document.getElementById('chat-send').addEventListener('click', async () => {
    const message = document.getElementById('chat-input').value;

    try {
        const response = await fetch('/AI/GetAIResponse', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ prompt: message })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        const chatMessages = document.getElementById('chat-messages');

        // Kullanıcı mesajı
        const userMessageElement = document.createElement('div');
        userMessageElement.classList.add('user-message');
        userMessageElement.textContent = message;
        chatMessages.appendChild(userMessageElement);

        // Yapay zeka cevabı
        const botMessageElement = document.createElement('div');
        botMessageElement.classList.add('bot-message');
        botMessageElement.textContent = data.choices[0].text;
        chatMessages.appendChild(botMessageElement);
        
        // Scroll to the bottom of the chat
        chatMessages.scrollTop = chatMessages.scrollHeight;
    } catch (error) {
        console.error('Error:', error);
        const chatMessages = document.getElementById('chat-messages');

        const errorElement = document.createElement('div');
        errorElement.classList.add('bot-message');
        errorElement.textContent = `Bir hata oluştu: ${error.message}`;
        chatMessages.appendChild(errorElement);
    } finally {
        document.getElementById('chat-input').value = '';
    }
});
