/**
 * api.js
 * Módulo responsável pela comunicação via fetch com a API REST backend.
 */

const API_BASE_URL = "http://localhost:5274/api"; // URL base do backend C# (Ajuste se necessário)

const api = {
    async request(endpoint, method = 'GET', body = null) {
        const headers = {
            'Content-Type': 'application/json'
        };

        const config = {
            method,
            headers,
        };

        if (body) {
            config.body = JSON.stringify(body);
        }

        try {
            const response = await fetch(`${API_BASE_URL}${endpoint}`, config);
            
            // Trata as respostas vazias (ex: 204 No Content ou se o body for nulo)
            const isJson = response.headers.get('content-type')?.includes('application/json');
            const data = isJson ? await response.json() : null;

            if (!response.ok) {
                // Monta o objeto de erro baseado no ProblemDetails retornado pela API
                const errorMessage = data?.detail || data?.title || 'Erro desconhecido na API';
                throw { status: response.status, message: errorMessage };
            }

            return data;
        } catch (error) {
            // Se for erro da rede (fetch failed) não terá status
            if (!error.status) {
                console.error("Network Error:", error);
                throw { status: 0, message: "Não foi possível conectar ao servidor. Verifique se a API está rodando." };
            }
            throw error;
        }
    },

    // --- AUTORES ---
    async getAutores() { return this.request('/autores'); },
    async getAutor(id) { return this.request(`/autores/${id}`); },
    async criarAutor(autorDto) { return this.request('/autores', 'POST', autorDto); },
    async atualizarAutor(id, autorDto) { return this.request(`/autores/${id}`, 'PUT', autorDto); },
    async excluirAutor(id) { return this.request(`/autores/${id}`, 'DELETE'); },

    // --- LIVROS ---
    async getLivros(titulo = '', autorId = '') {
        const queryParams = new URLSearchParams();
        if (titulo) queryParams.append('titulo', titulo);
        if (autorId) queryParams.append('autorId', autorId); // Backend espera int? autorId, mas passa query param
        
        const qs = queryParams.toString() ? `?${queryParams.toString()}` : '';
        return this.request(`/livros${qs}`);
    },
    async getLivro(id) { return this.request(`/livros/${id}`); },
    async criarLivro(livroDto) { return this.request('/livros', 'POST', livroDto); },
    async atualizarLivro(id, livroDto) { return this.request(`/livros/${id}`, 'PUT', livroDto); },
    async excluirLivro(id) { return this.request(`/livros/${id}`, 'DELETE'); },

    // --- ALUNOS ---
    async getAlunos() { return this.request('/alunos'); },
    async criarAluno(alunoDto) { return this.request('/alunos', 'POST', alunoDto); },
    async atualizarAluno(id, alunoDto) { return this.request(`/alunos/${id}`, 'PUT', alunoDto); },
    async excluirAluno(id) { return this.request(`/alunos/${id}`, 'DELETE'); },

    // --- EMPRÉSTIMOS ---
    async getTodosEmprestimos() { return this.request('/emprestimos'); },
    async getEmprestimosAbertos() { return this.request('/emprestimos/abertos'); },
    async getEmprestimosAluno(alunoId) { return this.request(`/emprestimos/aluno/${alunoId}`); },
    async criarEmprestimo(emprestimoDto) { return this.request('/emprestimos', 'POST', emprestimoDto); },
    async devolverEmprestimo(id) { return this.request(`/emprestimos/${id}/devolucao`, 'PUT'); }
};
