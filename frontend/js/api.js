const api = {
    baseUrl: '/api',

    // Helper para adicionar o header de Authorization
    getHeaders: () => {
        const token = sessionStorage.getItem('token');
        return {
            'Content-Type': 'application/json',
            ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        };
    },

    // Tratamento unificado de erros
    handleResponse: async (response) => {
        if (response.status === 401) {
            // Token expirado ou inválido
            sessionStorage.removeItem('token');
            sessionStorage.removeItem('user');
            window.location.href = 'index.html';
            return null;
        }
        if (!response.ok) {
            let errorMsg = 'Erro na requisição';
            try {
                const data = await response.json();
                if (data.errors && typeof data.errors === 'object') {
                    const errorList = Object.values(data.errors).flat();
                    errorMsg = errorList.join('\n') || data.detail || data.title || errorMsg;
                } else {
                    errorMsg = data.detail || data.message || data.title || errorMsg;
                }
            } catch (e) {
                errorMsg = response.statusText || errorMsg;
            }
            throw new Error(errorMsg);
        }
        // Retorna null para 204 No Content, ou o JSON para o resto
        return response.status === 204 ? null : await response.json();
    },

    async get(endpoint, params = {}) {
        const url = new URL(`${this.baseUrl}/${endpoint}`, window.location.origin);
        Object.keys(params).forEach(key => {
            if (params[key] !== undefined && params[key] !== null && params[key] !== '') {
                url.searchParams.append(key, params[key]);
            }
        });
        const response = await fetch(url, { headers: this.getHeaders() });
        return this.handleResponse(response);
    },

    async post(endpoint, data) {
        const response = await fetch(`${this.baseUrl}/${endpoint}`, {
            method: 'POST',
            headers: this.getHeaders(),
            body: JSON.stringify(data)
        });
        return this.handleResponse(response);
    },

    async put(endpoint, data) {
        const response = await fetch(`${this.baseUrl}/${endpoint}`, {
            method: 'PUT',
            headers: this.getHeaders(),
            body: data ? JSON.stringify(data) : null
        });
        return this.handleResponse(response);
    },

    async delete(endpoint) {
        const response = await fetch(`${this.baseUrl}/${endpoint}`, {
            method: 'DELETE',
            headers: this.getHeaders()
        });
        return this.handleResponse(response);
    }
};
