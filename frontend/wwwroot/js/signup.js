import { mostrarAlerta } from "./scripts.js";

const URL_BASE = "http://localhost:8080";

// Função de cadastro
async function userRegister({ name, userName, userEmail, password, userType }) {

    const submitButton = document.querySelector('button[type="submit"]');
    if (submitButton) {
        submitButton.disabled = true;
    }

    try {
        const response = await fetch(`${URL_BASE}/api/user/signup`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: "include",
            body: JSON.stringify({ name, userName, userEmail, password, userType })
        });

        const data = await response.json();

        if (!response.ok) {
            console.error("Erro ao cadastrar:", data.message || "Erro desconhecido");
            mostrarAlerta("Erro ao cadastrar o usuário");
        } else {
            mostrarAlerta("Cadastro realizado com sucesso!");
            window.location.href = "/index";
        }

        return data;
    } catch (error) {
        console.error(error);
        throw error;
    } finally {
        if (submitButton) {
            submitButton.disabled = false;
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('registerForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        try {
            await userRegister({
                name: document.getElementById('name').value.trim(),
                userName: document.getElementById('userName').value.trim(),
                userEmail: document.getElementById('userEmail').value.trim(),
                password: document.getElementById('password').value.trim(),
                userType: document.getElementById('userType').value.trim()
            });
        } catch (error) {}
    });
});

export { userRegister };