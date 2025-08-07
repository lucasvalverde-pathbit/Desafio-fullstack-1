import { mostrarAlerta } from "./scripts.js";

const URL_BASE = "http://localhost:8080";


// Verifica se o usuário é um administrador e se está autenticado
function isAdmin() {
    const userType = localStorage.getItem("userType");
    const token = localStorage.getItem("token");
    return userType === "Administrador" && token;
}

// Carrega os produtos
async function loadProducts() {
    if (!isAdmin()) {
        window.location.href = "/error";
        return; 
    }

    try {
        const response = await fetch(`${URL_BASE}/api/product`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("token")}`
            }
        });

        if (!response.ok) {
            throw new Error(`Erro ao carregar produtos: ${response.status}`);
        }

        const productsData = await response.json();

        const products = Array.isArray(productsData.$values) ? productsData.$values : [];

        const tableBody = document.querySelector("#productList tbody");
        tableBody.innerHTML = "";

        products.forEach(product => {
            const row = document.createElement("tr");
            row.innerHTML = `
                <td>${product.productName}</td>
                <td>${product.price}</td>
                <td>${product.quantityAvaliable}</td>
                <td>
                    <button class="editBtn" data-id="${product.id}">Editar</button>
                    <button class="deleteBtn" data-id="${product.id}">Excluir</button>
                </td>
            `;
            tableBody.appendChild(row);
        });

        document.querySelectorAll(".editBtn").forEach(button => {
            button.addEventListener("click", editProduct);
        });

        document.querySelectorAll(".deleteBtn").forEach(button => {
            button.addEventListener("click", deleteProduct);
        });

    } catch (error) {
        console.error("Erro ao carregar produtos:", error);
        mostrarAlerta(error.message);
    }
}

// Filtro de produtos
document.getElementById("productSearch").addEventListener("input", function () {
    const searchTerm = this.value.toLowerCase().trim();
    const rows = document.querySelectorAll("#productList tbody tr");

    rows.forEach(row => {
        const productName = row.querySelector("td:nth-child(1)").textContent.toLowerCase();
        const shouldShow = productName.includes(searchTerm);
        row.style.display = shouldShow ? "" : "none";
    });
});

// Editar produto
async function editProduct(event) {
    const id = event.target.dataset.id;
    const response = await fetch(`${URL_BASE}/api/product/${id}`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${localStorage.getItem("token")}`
        }
    });

    if (response.ok) {
        const product = await response.json();

        document.getElementById("id").value = product.id;
        document.getElementById("productName").value = product.productName;
        document.getElementById("price").value = product.price;
        document.getElementById("quantityAvaliable").value = product.quantityAvaliable;

        openModal("Editar Produto");
    } else {
        mostrarAlerta("Erro ao editar o produto.");
    }
}

// Excluir produto
async function deleteProduct(event) {
    const id = event.target.dataset.id;
    const response = await fetch(`${URL_BASE}/api/product/${id}`, {
        method: "DELETE",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${localStorage.getItem("token")}`
        }
    });

    if (response.ok) {
        mostrarAlerta("Produto excluído com sucesso.");
        loadProducts();
    } else {
        mostrarAlerta("Erro ao excluir o produto.");
    }
}

// Adicionar produto
document.getElementById("addProduct").addEventListener("click", () => {
    document.getElementById("id").value = "";
    document.getElementById("productName").value = "";
    document.getElementById("price").value = "";
    document.getElementById("quantityAvaliable").value = "";

    openModal("Adicionar Produto");
});

// Abre o modal de criar produto
let productModalInstance;

function openModal(title) {
    document.getElementById("modalTitle").textContent = title;

    const modalElement = document.getElementById("productModal");
    
    // Se ainda não criamos uma instância, criamos uma
    if (!productModalInstance) {
        productModalInstance = new bootstrap.Modal(modalElement);
    }

    productModalInstance.show();
}


// Mostra nome de usuário no perfil
document.addEventListener("DOMContentLoaded", () => {
    const name = localStorage.getItem("name");
    if (name) {
        document.getElementById("nameDisplay").textContent = name;
    }
    loadProducts();
});

// Redireciona para a página do perfil
document.getElementById("profileBtn").addEventListener("click", () => {
    window.location.href = "profile";
});

// Salva produto adicionado ou editado
document.getElementById("productForm").addEventListener("submit", async (event) => {
    event.preventDefault();

    const id = document.getElementById("id").value;
    const productName = document.getElementById("productName").value;
    const price = parseFloat(document.getElementById("price").value);
    const quantityAvaliable = parseInt(document.getElementById("quantityAvaliable").value);

    const method = id ? "PUT" : "POST";
    const url = id ? `${URL_BASE}/api/product/${id}` : `${URL_BASE}/api/product`;

    const productData = {
        productName,
        price,
        quantityAvaliable
    };

    if (id) {
        productData.id = id;
    }

    const response = await fetch(url, {
        method,
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${localStorage.getItem("token")}`
        },
        body: JSON.stringify(productData)
    });

    if (response.ok) {
        mostrarAlerta("Produto salvo com sucesso.");
        loadProducts();
        if (productModalInstance) {
            productModalInstance.hide();
        }
    } else {
        mostrarAlerta("Erro ao salvar o produto.");
    }
});

// Carrega os pedidos
async function loadOrders() {
    if (!isAdmin()) {
        window.location.href = "/error";
        return;
    }

    try {
        const response = await fetch(`${URL_BASE}/api/order/admin`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("token")}`
            }
        });

        if (!response.ok) {
            throw new Error(`Erro ao carregar pedidos: ${response.status}`);
        }

        const ordersData = await response.json();
        const orders = Array.isArray(ordersData.$values) ? ordersData.$values : [];

        const tableBody = document.querySelector("#orderListTable tbody");
        tableBody.innerHTML = "";

        orders.forEach(order => {
            
            const orderId = order.id;
            const orderProducts = order.orderProducts?.$values || [];
        
            let productsList = orderProducts.length > 0 
                ? orderProducts.map(item => {
                    const productName = item.productName || "Nome do produto não disponível";
                    return `<li>${productName}</li>`;
                }).join('')
                : '<li>Produtos não encontrados</li>';
        
            const row = document.createElement("tr");
            row.innerHTML = `
                <td>${orderId}</td>
                <td><ul>${productsList}</ul></td>
                <td>
                    <select class="statusSelect" data-id="${orderId}">
                        <option value="Pendente" ${order.status === "Pendente" ? "selected" : ""}>Pendente</option>
                        <option value="Enviado" ${order.status === "Enviado" ? "selected" : ""}>Enviado</option>
                    </select>
                </td>
                <td><button class="saveStatusBtn" data-id="${orderId}">Salvar</button></td>
            `;
            tableBody.appendChild(row);
        });        

        document.querySelectorAll(".saveStatusBtn").forEach(button => {
            button.addEventListener("click", updateOrderStatus);
        });
    } catch (error) {
        console.error("Erro ao carregar pedidos:", error);
    }
}

// Atualiza o status do pedido
async function updateOrderStatus(event) {
    const orderId = event.target.dataset.id;
    const statusSelect = document.querySelector(`.statusSelect[data-id='${orderId}']`);
    const newStatus = statusSelect.value;

    try {
        const response = await fetch(`${URL_BASE}/api/order/${orderId}/status`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("token")}`
            },
            body: JSON.stringify({
                request: { status: newStatus }
            })
        });

        if (!response.ok) {
            throw new Error(`Erro ao atualizar status: ${response.status}`);
        }

        mostrarAlerta("Status atualizado com sucesso!");
    } catch (error) {
        console.error("Erro ao atualizar status:", error);
        mostrarAlerta("Erro ao atualizar status.");
    }
}

document.querySelectorAll(".saveStatusBtn").forEach(button =>{
    button.addEventListener("click", updateOrderStatus);
});

// Abrir a lista de pedidos
document.getElementById("orderListBtn").addEventListener("click", () => {
document.getElementById("orderSection").classList.remove("d-none");
    loadOrders();
});

// Fechar lista de pedidos
document.getElementById("closeOrderSection").addEventListener("click", () => {
    document.getElementById("orderSection").classList.add("d-none");
});

// Verifica se está autenticado e carrega os produtos e os pedidos
document.addEventListener('DOMContentLoaded', () => {
    if (isAdmin()) {
        loadProducts();
        loadOrders();
    } else {
        window.location.href = "/error";
    }
});

// Logout
document.getElementById("logoutBtn").addEventListener("click", () => {
    ["token", "userEmail", "userType", "userId", "userName", "name"].forEach(item => localStorage.removeItem(item));
    setTimeout(() => window.location.href = "/index", 100);
});