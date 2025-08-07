export function mostrarAlerta(mensagem, tempo = 3000) {
    const alertBox = document.getElementById('customAlert');
    const alertMessage = document.getElementById('customAlertMessage');

    if (!alertBox || !alertMessage) return;

    alertMessage.textContent = mensagem;
    alertBox.classList.remove('d-none', 'fade-out');
    alertBox.classList.add('show');

    setTimeout(() => {
        alertBox.classList.add('fade-out');
        setTimeout(() => {
            alertBox.classList.remove('show');
            alertBox.classList.add('d-none');
        }, 500);
    }, tempo);
}