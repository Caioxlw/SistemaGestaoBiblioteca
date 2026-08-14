/**
 * auth.js
 * Módulo responsável pela autenticação mockada (sem JWT real)
 * Valida os usuários no client-side e gerencia o sessionStorage.
 */

const auth = {
    // Lista mockada de usuários
    users: {
        'admin': { password: 'admin123', role: 'admin', nome: 'Administrador do Sistema' },
        'aluno': { password: 'aluno123', role: 'aluno', nome: 'Aluno Padrão', alunoId: 1 } // Simulando que é o aluno ID 1
    },

    login(username, password) {
        const user = this.users[username];
        if (user && user.password === password) {
            const sessionData = {
                username: username,
                role: user.role,
                nome: user.nome,
                alunoId: user.alunoId || null
            };
            sessionStorage.setItem('session', JSON.stringify(sessionData));
            return true;
        }
        return false;
    },

    logout() {
        sessionStorage.removeItem('session');
        window.location.href = 'index.html';
    },

    getSession() {
        const session = sessionStorage.getItem('session');
        return session ? JSON.parse(session) : null;
    },

    getProfile() {
        const session = this.getSession();
        return session ? session.role : null;
    },

    requireAuth(allowedRole = null) {
        const session = this.getSession();
        if (!session) {
            window.location.href = 'index.html';
            return;
        }
        
        if (allowedRole && session.role !== allowedRole) {
            // Se tentar acessar página que não tem permissão, volta para login
            this.logout();
        }
        
        return session;
    },
    
    renderUserInfo() {
        const session = this.getSession();
        if (session) {
            const userNameEl = document.getElementById('navbarUserName');
            const userRoleEl = document.getElementById('navbarUserRole');
            if(userNameEl) userNameEl.textContent = session.nome;
            if(userRoleEl) userRoleEl.textContent = session.role === 'admin' ? '(Administrador)' : '(Aluno)';
        }
    }
};
