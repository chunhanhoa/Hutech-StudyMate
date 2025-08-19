class StudyChatBot {
    constructor() {
        this.currentStudyData = null;
        this.inlineChatSection = null;
        this.inlineMessagesContainer = null;
        this.inlineChatInput = null;
        this.inlineSendButton = null;
        this.chatHistory = [];
        this.isFirstInteraction = true; // Thêm flag để theo dõi lần đầu
        this.init();
    }

    init() {
        this.setupInlineChat();
        this.attachEventListeners();
    }

    setupInlineChat() {
        this.inlineChatSection = document.getElementById('aiChatSection');
        this.inlineMessagesContainer = document.getElementById('chatMessagesInline');
        this.inlineChatInput = document.getElementById('chatInputInline');
        this.inlineSendButton = document.getElementById('chatSendInline');
    }

    attachEventListeners() {
        if (this.inlineSendButton) {
            this.inlineSendButton.addEventListener('click', () => this.sendMessage());
        }
        
        if (this.inlineChatInput) {
            this.inlineChatInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    this.sendMessage();
                }
            });
        }
    }

    updateStudyData(data) {
        this.currentStudyData = data;
        
        // Đảm bảo chatbot inline được hiển thị
        if (this.inlineChatSection) {
            this.inlineChatSection.classList.remove('hidden');
        }
        
        if (data && data.grades && data.grades.length > 0) {
            // Xóa tin nhắn cũ và thêm tin nhắn chào mừng mới
            if (this.inlineMessagesContainer) {
                this.inlineMessagesContainer.innerHTML = '';
                this.chatHistory = [];
                this.isFirstInteraction = true; // Reset flag khi có dữ liệu mới
            }
            
            this.addMessage(`📊 Đã phân tích xong bảng điểm!

Tôi thấy bạn đã học ${data.grades.length} môn, GPA hiện tại ${data.summary.gpa4 ? data.summary.gpa4.toFixed(2) : 'N/A'}/4.0.

💡 Hỏi tôi về:
• "Tôi còn thiếu những môn nào để tốt nghiệp?"
• "Làm sao để cải thiện GPA?"  
• "Nên đăng ký môn gì kỳ tới?"
• "12 TC tự chọn thì nên chọn hướng nào?"

Hãy đặt câu hỏi cho tui nhé! 😊`, 'ai');
        }
    }

    async sendMessage() {
        const message = this.inlineChatInput.value.trim();
        if (!message) return;

        this.addMessage(message, 'user');
        this.inlineChatInput.value = '';
        this.inlineSendButton.disabled = true;

        const typingId = this.addTypingIndicator();

        try {
            const response = await fetch('/api/chat/ask', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    message: message,
                    studyData: this.currentStudyData,
                    chatHistory: this.chatHistory.slice(-6),
                    isFirstInteraction: this.isFirstInteraction // Gửi flag lần đầu
                })
            });

            this.removeTypingIndicator(typingId);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const data = await response.json();
            this.addMessage(data.response, 'ai');
            
            // Đánh dấu không còn là lần đầu nữa
            this.isFirstInteraction = false;

        } catch (error) {
            this.removeTypingIndicator(typingId);
            this.addMessage('❌ **Lỗi kết nối**\n\nXin lỗi, tôi không thể trả lời lúc này. Vui lòng thử lại sau.', 'ai');
            console.error('Chat error:', error);
        } finally {
            this.inlineSendButton.disabled = false;
            this.inlineChatInput.focus();
        }
    }

    // Function để convert markdown **text** thành HTML bold
    formatMessage(content) {
        if (!content) return '';
        
        // Convert **text** thành <strong>text</strong>
        return content.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
    }

    addMessage(content, type) {
        const container = this.inlineMessagesContainer;
        if (!container) return;

        // Format message để convert **text** thành bold
        const formattedContent = this.formatMessage(content);

        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${type}-message`;
        messageDiv.innerHTML = `
            <div class="message-content">${formattedContent}</div>
        `;
        container.appendChild(messageDiv);
        
        // Lưu vào lịch sử chat
        this.chatHistory.push({ role: type === 'user' ? 'user' : 'assistant', content: content });
        
        // Giới hạn lịch sử chat (chỉ giữ 10 tin nhắn gần nhất)
        if (this.chatHistory.length > 10) {
            this.chatHistory = this.chatHistory.slice(-10);
        }
        
        this.scrollToBottom();
    }

    addTypingIndicator() {
        const container = this.inlineMessagesContainer;
        if (!container) return Date.now();

        const typingId = Date.now();
        const typingDiv = document.createElement('div');
        typingDiv.className = 'message ai-message typing';
        typingDiv.id = `typing-${typingId}`;
        typingDiv.innerHTML = `
            <div class="message-content">
                <div class="typing-dots">
                    <span></span><span></span><span></span>
                </div>
            </div>
        `;
        container.appendChild(typingDiv);
        this.scrollToBottom();
        return typingId;
    }

    removeTypingIndicator(typingId) {
        const typingElement = document.getElementById(`typing-${typingId}`);
        if (typingElement) {
            typingElement.remove();
        }
    }

    scrollToBottom() {
        const container = this.inlineMessagesContainer;
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }
}

// Initialize chatbot
const studyChatBot = new StudyChatBot();

// Export for use in main app
window.studyChatBot = studyChatBot;
