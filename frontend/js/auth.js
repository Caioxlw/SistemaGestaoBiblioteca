const auth = {
    // Agora chama a API real
    async login(email, password) {
        try {
            const result = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, senha: password })
            });
            
            if (result.ok) {
                const data = await result.json();
                sessionStorage.setItem('token', data.token);
                sessionStorage.setItem('user', JSON.stringify({
                    id: data.userId,
                    nome: data.nome,
                    email: data.email,
                    perfil: data.perfil,
                    alunoId: data.alunoId
                }));
                return true;
            }
            return false;
        } catch (error) {
            console.error("Erro no login:", error);
            return false;
        }
    },

    logout() {
        sessionStorage.removeItem('token');
        sessionStorage.removeItem('user');
        window.location.href = 'index.html';
    },

    checkAuth(requiredProfile = null) {
        const token = sessionStorage.getItem('token');
        const userStr = sessionStorage.getItem('user');

        if (!token || !userStr) {
            window.location.href = 'index.html';
            return;
        }

        const user = JSON.parse(userStr);

        if (requiredProfile) {
            if (Array.isArray(requiredProfile)) {
                if (!requiredProfile.includes(user.perfil)) {
                    window.location.href = 'index.html';
                }
            } else if (user.perfil !== requiredProfile) {
                window.location.href = 'index.html';
            }
        }
        
        return user;
    },

    getUser() {
        const userStr = sessionStorage.getItem('user');
        return userStr ? JSON.parse(userStr) : null;
    },

    getProfile() {
        const user = this.getUser();
        return user ? user.perfil : null;
    }
};
