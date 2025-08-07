import { mostrarAlerta } from "./scripts.js";

const URL_BASE = "http://localhost:8080";

document.addEventListener("DOMContentLoaded", async () => {
    function isAuthenticated(){
        const token = localStorage.getItem("token");
        const userId = localStorage.getItem("userId");
        return userId && token;
    }
    
    const userId = localStorage.getItem("userId");

    if (!isAuthenticated() || !userId) {
        window.location.href = "/error";
        return;
    }

    // Buscar dados do usuário na API
    try {
        const response = await fetch(`${URL_BASE}/api/user/${userId}`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${localStorage.getItem("token")}`
            }
        });

        if (!response.ok) throw new Error("Erro ao carregar perfil");

        const user = await response.json();
        document.getElementById("nameDisplay").textContent = user.name;
        document.getElementById("userNameDisplay").textContent = user.userName;
        document.getElementById("userEmailDisplay").textContent = user.userEmail;
    } catch (error) {
        mostrarAlerta(error.message);
    }

    // Abre modal de editar perfil
    document.getElementById("editProfileBtn").addEventListener("click", () => {
        document.getElementById("editModal").style.display = "block";

        const name = document.getElementById("nameDisplay").textContent;
        const userName = document.getElementById("userNameDisplay").textContent;
        const userEmail = document.getElementById("userEmailDisplay").textContent;

        document.getElementById("name").value = name;
        document.getElementById("userName").value = userName;
        document.getElementById("userEmail").value = userEmail;
    });

    // Fecha o modal
    document.getElementById("cancelEdit").addEventListener("click", () => {
        document.getElementById("editModal").style.display = "none";
    });

    // Volta para a página inicial
    document.getElementById("backBtn").addEventListener("click", () => {
        const userType = localStorage.getItem("userType");
        if (userType === "Cliente") {
            window.location.href = "/client";
        } else {
            window.location.href = "/admin";
        }
    });
});

// Salvar alterações do perfil
document.getElementById("profileForm").addEventListener("submit", async (event) => {
    event.preventDefault();

    const userId = localStorage.getItem("userId");
    const name = document.getElementById("name").value;
    const userName = document.getElementById("userName").value;
    const userEmail = document.getElementById("userEmail").value;
    const newPassword = document.getElementById("newPassword").value;

    const updatedData = { name, userName, userEmail };
    if (newPassword) {
        updatedData.password = newPassword;
    } 

    try {
        const response = await fetch(`${URL_BASE}/api/user/update/${userId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("token")}`
            },
            body: JSON.stringify(updatedData)
        });

        if (!response.ok) throw new Error("Erro ao atualizar perfil");

        const userType = localStorage.getItem("userType");
        if (userType === 'Cliente') {
            const customerResponse = await fetch(`${URL_BASE}/api/customer/update/${userId}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                },
                body: JSON.stringify(updatedData)
            });

            if (!customerResponse.ok) throw new Error("Erro ao atualizar perfil");
        }

        mostrarAlerta("Perfil atualizado com sucesso!");
        localStorage.setItem("name", name);
        document.getElementById("nameDisplay").textContent = name;
        document.getElementById("userNameDisplay").textContent = userName;

        document.getElementById("editModal").style.display = "none";
    } catch (error) {
        mostrarAlerta(error.message);
    }
});

// Excluir conta
document.getElementById("deleteAccount").addEventListener("click", async () => {
    const userId = localStorage.getItem("userId");
    const confirmDelete = confirm("Tem certeza que deseja excluir sua conta?");
    
    if (confirmDelete) {
        try {
            const response = await fetch(`${URL_BASE}/api/user/${userId}`, {
                method: "DELETE",
                headers: {
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                }
            });

            if (!response.ok) throw new Error("Erro ao excluir conta");

            const userType = localStorage.getItem("userType");
            if (userType === "Cliente") {
                const customerResponse = await fetch(`${URL_BASE}/api/customer/${userId}`, {
                    method: "DELETE",
                    headers: {
                        "Authorization": `Bearer ${localStorage.getItem("token")}`
                    }
                });

                if (!customerResponse.ok) throw new Error("Erro ao excluir dados do cliente");
            }

            mostrarAlerta("Conta excluída com sucesso!");
            localStorage.clear();
            window.location.href = "/index";
        } catch (error) {
            mostrarAlerta(error.message);
        }
    }
});

// Voltar para a tela do cliente
document.getElementById("backBtn").addEventListener("click", () => {
    window.location.href = "/client";
});