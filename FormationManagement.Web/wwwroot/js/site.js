// Handles the "Ask the AI trainer" box on the lesson page (Views/Lesson/View.cshtml).
document.addEventListener('DOMContentLoaded', function () {
    const askForm = document.getElementById('ask-avatar-form');
    if (!askForm) return;

    askForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const lessonId = askForm.dataset.lessonId;
        const question = document.getElementById('avatar-question').value.trim();
        if (!question) return;

        const responseBox = document.getElementById('avatar-response-text');
        responseBox.textContent = 'Thinking...';

        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        const formData = new FormData();
        formData.append('lessonId', lessonId);
        formData.append('question', question);
        formData.append('__RequestVerificationToken', token);

        try {
            const res = await fetch('/Lesson/AskAvatar', { method: 'POST', body: formData });
            const data = await res.json();
            responseBox.textContent = data.responseText || 'No response from the AI trainer.';
        } catch (err) {
            responseBox.textContent = 'Something went wrong reaching the AI trainer.';
        }
    });
});
