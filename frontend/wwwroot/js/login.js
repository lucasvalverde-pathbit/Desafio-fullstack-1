import { mostrarAlerta } from "./scripts.js";
const URL_BASE = "http://localhost:8080";

// Função para decodificar o token JWT
function decodeToken(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        const decoded = JSON.parse(jsonPayload);
        return decoded;
    } catch (error) {
        console.error("Erro ao decodificar token:", error);
        return null;
    }
}

async function userLogin({ userEmail, password }) {
    try {
        const response = await fetch(`${URL_BASE}/api/user/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ userEmail, password })
        });

        let data = await response.json();

        if (!response.ok) {
            console.error("Erro no login:", data.message || "Credenciais inválidas");
            return;
        }

        console.log("Login realizado com sucesso!");

        const decodedToken = decodeToken(data.token);
        if (!decodedToken) {
            console.error("Falha ao decodificar o token.");
            return;
        }

        const userType = decodedToken.userType;
        const userId = decodedToken.sub;
        const userName = decodedToken.userName;
        const name = decodedToken.name;
        const email = decodedToken.email;

        if (userId) {
            localStorage.setItem("token", data.token);
            localStorage.setItem("userEmail", email);
            localStorage.setItem("userType", userType);
            localStorage.setItem("userId", userId);
            localStorage.setItem("userName", userName);
            localStorage.setItem("name", name);

            // Se o usuário for do tipo Cliente, salva as informações do customer
            if (userType === 'Cliente') {
                localStorage.setItem("customerName", name);
                localStorage.setItem("customerEmail", email);
            }
        } else {
            console.error("userId não encontrado no token.");
        }

        setTimeout(() => {
            // Redireciona com base no tipo de usuário
            if (userType === 'Administrador') {
                window.location.href = "/admin";
            } else if (userType === 'Cliente') {
                window.location.href = "/client";
            } else {
                console.warn("Tipo de usuário desconhecido:", userType);
            }
        }, 500);

    } catch (error) {
        console.error("Erro na requisição de login:", error);
        mostrarAlerta("Erro ao fazer login");
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('loginForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        try {
            await userLogin({
                userEmail: document.getElementById('email').value.trim(),
                password: document.getElementById('passwordLogin').value.trim()
            });
        } catch (error) {}
    });
});

export { userLogin };