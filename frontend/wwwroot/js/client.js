import { mostrarAlerta } from './scripts.js';

const URL_BASE = "http://localhost:8080";

// Verifica se o usuário é cliente e está autenticado
function isClient() {
    const userType = localStorage.getItem("userType");
    const token = localStorage.getItem("token");
    return userType === "Cliente" && token;
}

// Carrega produtos e pedidos
async function loadData() {
    if (!isClient()) {
        window.location.href = "/error";
        return;
    }

    try {
        const [products, orders] = await Promise.all([getProducts(), getOrders()]);
        renderProductOptions(products);
        renderOrders(orders);
    } catch (error) {
        mostrarAlerta("Erro ao carregar dados: " + error.message);
    }
}

// Busca pedidos
async function getOrders() {
    try {
        const response = await fetch(`${URL_BASE}/api/order/client`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("token")}`
            }
        });

        if (!response.ok) {
            throw new Error("Falha ao carregar pedidos");
        }

        const data = await response.json();
        return data.$values || data;
    } catch (error) {
        console.error("Erro ao obter pedidos:", error);
        return [];
    }
}

// Busca produtos
async function getProducts() {
    try {
        const response = await fetch(`${URL_BASE}/api/product`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("token")}`
            },
        });

        if (!response.ok) throw new Error("Falha ao carregar produtos");

        const data = await response.json();
        return data.$values || data;
    } catch (error) {
        console.error("Erro ao obter produtos:", error);
        return [];
    }
}

// Busca endereço pelo CEP
async function getAddress(cep) {
    if (!cep) {
        mostrarAlerta("Insira um CEP.");
        return;
    }
    
    try {
        const response = await fetch(`${URL_BASE}/api/order/address/${cep}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("token")}`
            }
        });

        if (!response.ok) throw new Error("CEP inválido.");

        const data = await response.json();
        if (data.address) {
            document.getElementById("address").value = data.address || "Endereço não encontrado.";
        }
    } catch (error) {
        mostrarAlerta(error.message);
    }
}

// Renderiza opções de produtos no select
function renderProductOptions(products) {
    const productSelect = document.getElementById("product-select");
    if (!productSelect) {
        console.error("Elemento 'product-select' não encontrado.");
        return;
    }
    productSelect.innerHTML = "";

    products.forEach(product => {
        const option = document.createElement("option");
        option.value = product.id;
        option.textContent = `${product.productName} - R$ ${product.price.toFixed(2)}`;
        option.dataset.price = product.price;
        productSelect.appendChild(option);
    });
}

// Mostra nome do usuário no perfil
document.addEventListener("DOMContentLoaded", () => {
    const customerName = localStorage.getItem("customerName"); // Usa customerName
    if (customerName) {
        document.getElementById("nameDisplay").textContent = customerName;
    }

    loadData();
});

// Redireciona para o perfil
document.getElementById("profileBtn").addEventListener("click", () => {
    window.location.href = "/profile";
});

// Abre modal de criar pedido
document.getElementById("createOrderBtn").addEventListener("click", () => {
    document.getElementById("orderModal").style.display = "block";
});

// Fecha o modal
document.getElementById("cancelOrder").addEventListener("click", () => {
    document.getElementById("orderModal").style.display = "none";
});

// Busca endereço ao perder foco no campo CEP
document.getElementById("cep").addEventListener("blur", (event) => {
    const cep = event.target.value.trim();
    if (cep.length === 8) getAddress(cep);
});

// Adiciona mais produtos ao pedido
document.getElementById("addProductBtn").addEventListener("click", () => {
    const productContainer = document.createElement("div");
    productContainer.classList.add("product-entry");  // Adiciona o produto na div com a classe product-entry

    // Criar label e select do produto
    const productLabel = document.createElement("label");
    productLabel.textContent = "Produto:";
    const productSelectId = `product-select-${Date.now()}`; // ID único para cada select
    productLabel.setAttribute("for", productSelectId);

    const productSelect = document.createElement("select");
    productSelect.classList.add("product-select");
    productSelect.setAttribute("id", productSelectId);
    productSelect.setAttribute("name", "productSelect");
    productSelect.required = true;

    // Copiar opções do select original
    const originalSelect = document.getElementById("product-select");
    if (originalSelect) {
        Array.from(originalSelect.options).forEach(option => {
            const newOption = option.cloneNode(true);
            productSelect.appendChild(newOption);
        });
    }

    // Criar label e input da quantidade
    const quantityLabel = document.createElement("label");
    quantityLabel.textContent = "Quantidade:";
    const quantityInputId = `quantity-input-${Date.now()}`; // ID único para cada input
    quantityLabel.setAttribute("for", quantityInputId);

    const quantityInput = document.createElement("input");
    quantityInput.type = "number";
    quantityInput.classList.add("quantity-input");
    quantityInput.setAttribute("id", quantityInputId);
    quantityInput.setAttribute("name", "quantityInput");
    quantityInput.setAttribute("autocomplete", "off");
    quantityInput.min = "1";
    quantityInput.required = true;

    // Criar botão de remover produto do pedido
    const removeButton = document.createElement("button");
    removeButton.textContent = "Excluir";
    removeButton.classList.add("remove-product-btn");
    removeButton.style.marginTop = "5px";
    removeButton.addEventListener("click", () => productContainer.remove());

    // Adicionar elementos ao container
    productContainer.append(productLabel, productSelect, quantityLabel, quantityInput, removeButton);

    // Adicionar o container ao formulário
    const createOrderForm = document.getElementById("createOrderForm");
    if (createOrderForm) {
        createOrderForm.insertBefore(productContainer, document.getElementById("submitOrderBtn"));
    }
});

// Submete o pedido
document.getElementById("createOrderForm").addEventListener("submit", async (event) => {
    event.preventDefault();

    const customerId = localStorage.getItem("userId");
    const customerName = localStorage.getItem("customerName");
    const customerEmail = localStorage.getItem("customerEmail");

    if (!customerId || !customerName || !customerEmail) {
        mostrarAlerta("Cliente não encontrado.");
        return;
    }

    // Verifique se pelo menos um produto foi adicionado
    let hasProductSelected = false;
    const productEntries = document.querySelectorAll(".product-entry");
    productEntries.forEach((entry) => {
        const productSelect = entry.querySelector(".product-select");
        const quantityInput = entry.querySelector(".quantity-input");
        const productId = productSelect.value;
        const quantity = quantityInput.value;
        if (productId && quantity) {
            hasProductSelected = true;
        }
    });

    if (!hasProductSelected) {
        mostrarAlerta("Adicione pelo menos um produto ao pedido.");
        return;
    }

    // Captura os dados de todos os produtos selecionados
    const orderProducts = [];
    let total = 0;

    productEntries.forEach((productEntry) => {
        const productSelect = productEntry.querySelector(".product-select");
        const quantityInput = productEntry.querySelector(".quantity-input");

        if (productSelect && quantityInput && quantityInput.value) {
            const productId = productSelect.value;
            const selectedOption = productSelect.options[productSelect.selectedIndex];
            const productName = selectedOption ? selectedOption.textContent.split(" - ")[0] : "Produto Desconhecido";
            const quantity = parseInt(quantityInput.value, 10);
            const price = parseFloat(productSelect.options[productSelect.selectedIndex].dataset.price);

            if (productId && quantity > 0) {
                const subtotal = price * quantity;
                total += subtotal;

                orderProducts.push({
                    productId,
                    productName,
                    quantity,
                    price
                });
            }
        }
    });

    if (orderProducts.length === 0) {
        mostrarAlerta("Por favor, adicione um produto válido ao pedido.");
        return;
    }

    // Estrutura do pedido
    const orderData = {
        customerId: localStorage.getItem("userId"),
        cep: document.getElementById("cep").value,
        address: `${document.getElementById("address").value}, ${document.getElementById("houseNumber").value}`,
        total,
        customer: {
            id: localStorage.getItem("userId"),
            customerName: localStorage.getItem("customerName"),
            customerEmail: localStorage.getItem("customerEmail")
        },
        products: orderProducts
    };

    try {
        const response = await fetch(`${URL_BASE}/api/order`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("token")}` 
            },
            body: JSON.stringify(orderData)
        });

        if (response.ok) {
            mostrarAlerta("Pedido criado com sucesso!");
            loadData();
            document.getElementById("orderModal").style.display = "none";
        } else {
            const errorResponse = await response.json();
            mostrarAlerta(`Erro ao criar pedido: ${errorResponse.message}`);
        }
    } catch (error) {
        mostrarAlerta("Erro ao criar pedido: " + error.message);
    }
});

// Renderiza pedidos
function renderOrders(data) {
    const orders = data.$values || data;
    const ordersList = document.getElementById('ordersList');
    ordersList.innerHTML = '';

    if (orders.length === 0) {
        ordersList.innerHTML = '<p>Você ainda não tem pedidos.</p>';
        return;
    }

    orders.forEach((order) => {
        const orderDiv = document.createElement('div');
        orderDiv.classList.add('order');

        const products = order.orderProducts?.$values || [];
        let productsList = products.length > 0 
            ? products.map(item => {
                const productName = item.productName ? item.productName : "Nome do produto não disponível";
                return `<li>${productName} - Quantidade: ${item.quantity}</li>`;
            }).join('')
            : '<li>Produtos não encontrados</li>';

        const totalFormatted = order.total !== undefined ? `R$ ${order.total.toFixed(2)}` : "Indisponível";

        orderDiv.innerHTML = 
            `<h3>Pedido #${order.id}</h3>
            <p>Endereço: ${order.address}</p>
            <p>Data: ${new Date(order.orderDate).toLocaleDateString("pt-BR")}</p>
            <p>Status: ${order.status}</p>
            <p><strong>Total: ${totalFormatted}</strong></p>
            <ul>${productsList}</ul>`;

        if (order.status === "Pendente") {
            const deleteButton = document.createElement("button");
            deleteButton.textContent = "Excluir Pedido";
            deleteButton.classList.add("delete-order-btn");
            deleteButton.style.marginTop = "10px";
            deleteButton.addEventListener("click", () => deleteOrder(order.id));

            orderDiv.appendChild(deleteButton);
        }

        ordersList.appendChild(orderDiv);
    });
}

async function deleteOrder(orderId) {
    if (!confirm("Tem certeza que deseja excluir este pedido?")) return;

    try {
        const response = await fetch(`${URL_BASE}/api/order/${orderId}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${localStorage.getItem("token")}`
            }
        });

        if (!response.ok) throw new Error("Erro ao excluir pedido");

        mostrarAlerta("Pedido excluído com sucesso!");
        loadData(); // Recarrega a lista de pedidos
    } catch (error) {
        mostrarAlerta("Erro ao excluir pedido: " + error.message);
    }
}

// Logout
document.getElementById("logoutBtn").addEventListener("click", () => {
    ["token", "userEmail", "userType", "userId", "userName", "name", "customerName", "customerEmail"].forEach(item => localStorage.removeItem(item));
    setTimeout(() => window.location.href = "/index", 100);
});