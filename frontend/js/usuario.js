let pageCatalogo = 1;

document.addEventListener('DOMContentLoaded', () => {
    // Verifica Auth (Somente Aluno)
    const user = auth.checkAuth('Aluno');
    if (!user) return;

    document.getElementById('userName').textContent = user.nome;

    // Configuração do Menu Lateral
    const menuItems = document.querySelectorAll('.menu-item');
    const sections = document.querySelectorAll('.content-section');

    menuItems.forEach(item => {
        item.addEventListener('click', () => {
            menuItems.forEach(i => i.classList.remove('active'));
            sections.forEach(s => s.classList.remove('active'));

            item.classList.add('active');
            const sectionId = item.getAttribute('data-section');
            document.getElementById(`sec-${sectionId}`).classList.add('active');

            carregarSecao(sectionId);
        });
    });

    // Eventos
    document.getElementById('buscaCatalogo').addEventListener('input', debounce(() => {
        pageCatalogo = 1;
        carregarCatalogo();
    }, 500));
    
    document.getElementById('btnCatalogoAnt').addEventListener('click', () => { if(pageCatalogo > 1) { pageCatalogo--; carregarCatalogo(); } });
    document.getElementById('btnCatalogoProx').addEventListener('click', () => { pageCatalogo++; carregarCatalogo(); });

    // Inicializa
    carregarSecao('catalogo');
    atualizarBadgeNotificacoes();
});

// Helper: Formatação de Data Brasileira segura contra Timezone Shift
function formatarDataBR(dataStr) {
    if (!dataStr) return '-';
    if (typeof dataStr === 'string' && dataStr.includes('-')) {
        const [datePart] = dataStr.split('T');
        const partes = datePart.split('-');
        if (partes.length === 3) {
            const [ano, mes, dia] = partes;
            return `${dia}/${mes}/${ano}`;
        }
    }
    const d = new Date(dataStr);
    return isNaN(d.getTime()) ? '-' : d.toLocaleDateString('pt-BR');
}

// Helper: Debounce
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => { clearTimeout(timeout); func(...args); };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Router
function carregarSecao(secao) {
    switch (secao) {
        case 'catalogo': carregarCatalogo(); break;
        case 'meus-emprestimos': carregarMeusEmprestimos(); break;
        case 'minhas-reservas': carregarMinhasReservas(); break;
        case 'minhas-notificacoes': carregarNotificacoes(); break;
    }
}

// =================== CATÁLOGO ===================
async function carregarCatalogo() {
    document.getElementById('loaderCatalogo').style.display = 'flex';
    document.getElementById('gridLivros').style.display = 'none';

    try {
        const termo = document.getElementById('buscaCatalogo').value;
        const res = await api.get(`livros`, { termo: termo, page: pageCatalogo, pageSize: 8 });
        
        const grid = document.getElementById('gridLivros');
        grid.innerHTML = '';
        
        res.itens.forEach(l => {
            const esgotado = l.quantidade <= 0;
            const estoqueBadge = esgotado ? 
                '<span class="badge badge-danger">Esgotado</span>' : 
                `<span class="badge badge-success">${l.quantidade} Disp.</span>`;
            
            const actionButton = esgotado ? 
                `<button class="btn btn-small btn-warning" onclick="reservar(${l.id})">Reservar</button>` :
                `<span style="font-size:0.8rem; color:var(--text-muted)">Vá à biblioteca</span>`;

            grid.innerHTML += `
                <div class="book-card glass-effect">
                    <div class="book-header">
                        <div class="book-icon">📚</div>
                        ${estoqueBadge}
                    </div>
                    <div class="book-title">${l.titulo}</div>
                    <div class="book-author">por ${l.nomeAutor}</div>
                    <div class="book-desc" title="${l.descricao}">${l.descricao || 'Nenhuma descrição disponível.'}</div>
                    <div class="book-meta">
                        <span>${l.categoria}</span>
                        <span>${l.anoPublicacao}</span>
                        <span>${l.localizacao}</span>
                    </div>
                    <div class="book-footer">
                        ${actionButton}
                    </div>
                </div>`;
        });

        document.getElementById('infoPaginaCatalogo').textContent = `Página ${res.paginaAtual} de ${res.totalPaginas}`;
        document.getElementById('btnCatalogoAnt').disabled = res.paginaAtual <= 1;
        document.getElementById('btnCatalogoProx').disabled = res.paginaAtual >= res.totalPaginas;

    } catch (e) { 
        mostrarAlerta(e.message, 'error'); 
    } finally {
        document.getElementById('loaderCatalogo').style.display = 'none';
        document.getElementById('gridLivros').style.display = 'grid';
    }
}

async function reservar(livroId) {
    if(!confirm('Deseja entrar na fila de reserva para este livro?')) return;
    try {
        const user = auth.getUser();
        await api.post('reservas', { alunoId: user.alunoId, livroId: livroId });
        mostrarAlerta('Reserva efetuada com sucesso! Você foi adicionado à fila.', 'success');
        carregarCatalogo();
    } catch (e) {
        mostrarAlerta(e.message, 'error');
    }
}

// =================== MEUS EMPRÉSTIMOS ===================
async function carregarMeusEmprestimos() {
    const tbody = document.querySelector('#tabelaMeusEmprestimos tbody');
    tbody.innerHTML = '';
    document.getElementById('loaderEmprestimos').style.display = 'flex';
    
    try {
        const user = auth.getUser();
        if (!user || !user.alunoId) {
            tbody.innerHTML = '<tr><td colspan="6" style="text-align:center">Nenhum perfil de aluno vinculado a esta conta.</td></tr>';
            return;
        }

        const emprestimos = await api.get(`emprestimos/aluno/${user.alunoId}`);
        tbody.innerHTML = '';
        
        if (!emprestimos || emprestimos.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" style="text-align:center">Nenhum empréstimo encontrado.</td></tr>';
        } else {
            emprestimos.forEach(e => {
                const statusBadge = e.status === 'Ativo' ? 
                    (e.diasAtraso > 0 ? `<span class="badge badge-danger">Atrasado (${e.diasAtraso}d)</span>` : '<span class="badge badge-warning">Ativo</span>') : 
                    '<span class="badge badge-success">Devolvido</span>';
                
                const multaStr = (e.multa && e.multa > 0) ? `<strong style="color:var(--danger)">R$ ${Number(e.multa).toFixed(2)}</strong>` : '-';
                const devolucaoStr = e.dataDevolucao ? formatarDataBR(e.dataDevolucao) : '-';
                const dataEmpStr = formatarDataBR(e.dataEmprestimo);
                const dataPrevStr = formatarDataBR(e.dataPrevistaDevolucao);

                tbody.innerHTML += `
                    <tr>
                        <td><strong>${e.tituloLivro || 'Livro sem título'}</strong></td>
                        <td>${dataEmpStr}</td>
                        <td>${dataPrevStr}</td>
                        <td>${devolucaoStr}</td>
                        <td>${statusBadge}</td>
                        <td>${multaStr}</td>
                    </tr>`;
            });
        }
    } catch (e) { 
        tbody.innerHTML = `<tr><td colspan="6" style="text-align:center; color:var(--danger)">Erro ao carregar empréstimos: ${e.message}</td></tr>`;
        mostrarAlerta(e.message, 'error'); 
    } finally {
        document.getElementById('loaderEmprestimos').style.display = 'none';
    }
}

// =================== MINHAS RESERVAS ===================
async function carregarMinhasReservas() {
    document.getElementById('loaderReservas').style.display = 'flex';
    try {
        const user = auth.getUser();
        const reservas = await api.get(`reservas/aluno/${user.alunoId}`);
        const tbody = document.querySelector('#tabelaMinhasReservas tbody');
        tbody.innerHTML = '';
        
        if (reservas.length === 0) {
            tbody.innerHTML = '<tr><td colspan="3" style="text-align:center">Nenhuma reserva encontrada.</td></tr>';
        } else {
            reservas.forEach(r => {
                let badgeClass = 'badge-warning';
                if(r.status === 'Atendida') badgeClass = 'badge-success';
                if(r.status === 'Cancelada') badgeClass = 'badge-danger';

                tbody.innerHTML += `
                    <tr>
                        <td><strong>${r.tituloLivro}</strong></td>
                        <td>${new Date(r.dataReserva).toLocaleString()}</td>
                        <td><span class="badge ${badgeClass}">${r.status}</span></td>
                    </tr>`;
            });
        }
    } catch (e) { 
        mostrarAlerta(e.message, 'error'); 
    } finally {
        document.getElementById('loaderReservas').style.display = 'none';
    }
}

// Helper: Alertas
function mostrarAlerta(msg, tipo) {
    const alertDiv = document.getElementById('alertMsg');
    alertDiv.textContent = msg;
    alertDiv.className = `alert alert-${tipo}`;
    alertDiv.style.display = 'block';
    window.scrollTo(0, 0);
    setTimeout(() => { alertDiv.style.display = 'none'; }, 5000);
}

// =================== NOTIFICAÇÕES ===================
async function atualizarBadgeNotificacoes() {
    const user = auth.getUser();
    if (!user || !user.alunoId) return;

    try {
        const notificacoes = await api.get(`notificacoes/aluno/${user.alunoId}`);
        const naoLidas = notificacoes ? notificacoes.filter(n => !n.lida).length : 0;
        const badge = document.getElementById('badgeNotificacoes');
        if (badge) {
            if (naoLidas > 0) {
                badge.textContent = naoLidas;
                badge.style.display = 'inline-block';
            } else {
                badge.style.display = 'none';
            }
        }
    } catch(e) {
        console.warn('Não foi possível verificar notificações:', e);
    }
}

async function carregarNotificacoes() {
    const user = auth.getUser();
    if (!user || !user.alunoId) return;

    const loader = document.getElementById('loaderNotificacoes');
    if (loader) loader.style.display = 'block';

    try {
        const notificacoes = await api.get(`notificacoes/aluno/${user.alunoId}`);
        const container = document.getElementById('listaNotificacoes');
        container.innerHTML = '';

        await atualizarBadgeNotificacoes();

        if (!notificacoes || notificacoes.length === 0) {
            container.innerHTML = '<p style="color:var(--text-muted); padding: 1rem 0;">Você não possui notificações no momento.</p>';
            return;
        }

        notificacoes.forEach(n => {
            const dataStr = new Date(n.dataNotificacao).toLocaleString();
            container.innerHTML += `
                <div style="padding: 1.25rem; border-radius: var(--radius-md); background: rgba(255,255,255,0.04); border-left: 4px solid var(--primary); display:flex; justify-content:space-between; align-items:center; gap: 1rem;">
                    <div>
                        <div style="font-size: 0.85rem; color: var(--text-muted); margin-bottom: 0.35rem;">${dataStr} &bull; <span class="badge badge-primary">${n.tipo}</span></div>
                        <div style="color: #fff; font-size: 0.95rem; line-height: 1.4;">${n.mensagem}</div>
                    </div>
                    <div>
                        ${!n.lida 
                            ? `<button class="btn btn-small btn-primary" onclick="marcarLida(${n.id})">Marcar como Lida</button>` 
                            : '<span class="badge badge-success">Lida</span>'}
                    </div>
                </div>
            `;
        });
    } catch (e) {
        mostrarAlerta('Erro ao carregar notificações: ' + e.message, 'error');
    } finally {
        if (loader) loader.style.display = 'none';
    }
}

async function marcarLida(id) {
    const user = auth.getUser();
    if (!user || !user.alunoId) return;
    try {
        await api.put(`notificacoes/${id}/lida?alunoId=${user.alunoId}`);
        await carregarNotificacoes();
    } catch(e) {
        mostrarAlerta('Erro ao marcar notificação como lida: ' + e.message, 'error');
    }
}
