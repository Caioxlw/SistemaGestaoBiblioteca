/**
 * usuario.js
 * Lógica do painel do aluno (visualização de catálogo e mock de empréstimos)
 */

document.addEventListener('DOMContentLoaded', () => {
    // 1. Proteger Rota (Apenas aluno)
    const session = auth.requireAuth('aluno');
    auth.renderUserInfo();

    // 2. Lógica das Abas
    const tabBtns = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    tabBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            tabBtns.forEach(b => b.classList.remove('active'));
            tabContents.forEach(c => c.classList.remove('active'));
            
            btn.classList.add('active');
            document.getElementById(btn.dataset.target).classList.add('active');
        });
    });

    // 3. Catálogo de Livros
    const livrosGrid = document.getElementById('livrosGrid');
    const loader = document.getElementById('loader');

    async function loadCatalogo(titulo = '', autor = '') {
        livrosGrid.innerHTML = '';
        loader.style.display = 'block';

        try {
            const livros = await api.getLivros(titulo, autor);
            loader.style.display = 'none';

            if (livros.length === 0) {
                livrosGrid.innerHTML = '<p style="color: var(--text-muted); grid-column: 1 / -1; text-align: center;">Nenhum livro encontrado.</p>';
                return;
            }

            livros.forEach(l => {
                const isDisponivel = l.quantidade > 0;
                const statusColor = isDisponivel ? 'var(--success)' : 'var(--danger)';
                const statusText = isDisponivel ? `${l.quantidade} disponível(is)` : 'Esgotado';

                livrosGrid.innerHTML += `
                    <div class="book-card glass-effect">
                        <div class="book-icon">📖</div>
                        <div class="book-title">${l.titulo}</div>
                        <div class="book-author">Por: ${l.nomeAutor}</div>
                        <div style="font-size: 0.8rem; color: var(--text-muted)">ISBN: ${l.isbn}</div>
                        <div class="book-stock" style="color: ${statusColor}">
                            ${statusText}
                        </div>
                    </div>
                `;
            });
        } catch (error) {
            loader.style.display = 'none';
            livrosGrid.innerHTML = `<p style="color: var(--danger);">Erro ao carregar catálogo: ${error.message}</p>`;
        }
    }

    // Handlers de Busca
    document.getElementById('btnBuscar').addEventListener('click', () => {
        const titulo = document.getElementById('buscaTitulo').value;
        const autor = document.getElementById('buscaAutor').value;
        loadCatalogo(titulo, autor);
    });

    // 4. Meus Empréstimos (Real via API)
    async function loadEmprestimos() {
        const list = document.getElementById('emprestimosList');
        list.innerHTML = '<div class="loader" style="display:block"></div>';
        
        try {
            if (!session || !session.alunoId) {
                list.innerHTML = '<p style="color: var(--text-muted);">Você precisa estar logado como um aluno com ID válido para ver empréstimos.</p>';
                return;
            }

            const emprestimos = await api.getEmprestimosAluno(session.alunoId);
            list.innerHTML = '';

            if (emprestimos.length === 0) {
                list.innerHTML = '<p style="color: var(--text-muted);">Você não possui empréstimos registrados.</p>';
                return;
            }

            emprestimos.forEach(emp => {
                let badgeClass = '';
                if (emp.status === 'Devolvido') badgeClass = 'badge-success';
                else if (emp.status === 'Ativo') badgeClass = 'badge-primary';
                else if (emp.status === 'Atrasado') badgeClass = 'badge-danger';

                const dateStr = new Date(emp.dataPrevistaDevolucao).toLocaleDateString('pt-BR');

                list.innerHTML += `
                    <div class="emprestimo-card">
                        <div class="emprestimo-info">
                            <h4>${emp.tituloLivro}</h4>
                            <p>ID Empréstimo: ${emp.id} | Devolução Prevista: ${dateStr}</p>
                        </div>
                        <div>
                            <span class="badge ${badgeClass}">${emp.status}</span>
                        </div>
                    </div>
                `;
            });
        } catch(error) {
            list.innerHTML = `<p style="color: var(--danger);">Erro ao carregar empréstimos: ${error.message}</p>`;
        }
    }

    // Inicialização
    loadCatalogo();
    loadEmprestimos();
});
