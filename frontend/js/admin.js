// Variáveis globais de controle
let pageLivros = 1;
let pageAuditoria = 1;
let chartCategoriaInstance = null;
let chartPopularesInstance = null;
let chartEmprestimosMesInstance = null;
let chartStatusAtrasosInstance = null;

// Inicialização
document.addEventListener('DOMContentLoaded', () => {
    // Verifica auth (Aceita Admin ou Bibliotecario)
    const user = auth.checkAuth(['Admin', 'Bibliotecario']);
    if (!user) return;

    document.getElementById('userName').textContent = user.nome;
    document.getElementById('userRole').textContent = user.perfil;

    // Se for admin, exibe a aba de auditoria
    if (user.perfil === 'Admin') {
        document.getElementById('menuAuditoria').style.display = 'block';
    } else {
        document.getElementById('menuAuditoria').style.display = 'none';
    }

    // Configuração do Menu Lateral (Sidebar)
    const menuItems = document.querySelectorAll('.menu-item');
    const sections = document.querySelectorAll('.content-section');

    menuItems.forEach(item => {
        item.addEventListener('click', () => {
            menuItems.forEach(i => i.classList.remove('active'));
            sections.forEach(s => s.classList.remove('active'));

            item.classList.add('active');
            const sectionId = item.getAttribute('data-section');
            document.getElementById(`sec-${sectionId}`).classList.add('active');

            // Carrega os dados da sessão selecionada
            carregarSecao(sectionId);
        });
    });

    // Eventos de Busca e Paginação
    document.getElementById('buscaLivro').addEventListener('input', debounce(() => {
        pageLivros = 1;
        carregarLivros();
    }, 500));
    
    document.getElementById('btnLivrosAnt').addEventListener('click', () => { if(pageLivros > 1) { pageLivros--; carregarLivros(); } });
    document.getElementById('btnLivrosProx').addEventListener('click', () => { pageLivros++; carregarLivros(); });
    
    document.getElementById('btnAuditoriaAnt').addEventListener('click', () => { if(pageAuditoria > 1) { pageAuditoria--; carregarAuditoria(); } });
    document.getElementById('btnAuditoriaProx').addEventListener('click', () => { pageAuditoria++; carregarAuditoria(); });

    // Formulários
    document.getElementById('formLivro').addEventListener('submit', async (e) => {
        e.preventDefault();
        await salvarLivro();
    });
    document.getElementById('formAutor').addEventListener('submit', async (e) => {
        e.preventDefault();
        await salvarAutor();
    });
    document.getElementById('formAluno').addEventListener('submit', async (e) => {
        e.preventDefault();
        await salvarAluno();
    });
    document.getElementById('formEmprestimo').addEventListener('submit', async (e) => {
        e.preventDefault();
        await salvarEmprestimo();
    });

    // Inicializa Dashboard
    carregarSecao('dashboard');
});

// Helper: Debounce
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => { clearTimeout(timeout); func(...args); };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Router simples das seções
function carregarSecao(secao) {
    switch (secao) {
        case 'dashboard': carregarDashboard(); break;
        case 'livros': carregarLivros(); carregarAutoresSelect(); break;
        case 'autores': carregarAutores(); break;
        case 'alunos': carregarAlunos(); break;
        case 'emprestimos': carregarEmprestimos(); break;
        case 'reservas': carregarSelectLivrosParaReserva(); break;
        case 'auditoria': carregarAuditoria(); break;
    }
}

// =================== DASHBOARD ===================
async function carregarDashboard() {
    try {
        const dash = await api.get('dashboard');
        
        // Cards
        document.getElementById('dashTotalLivros').textContent = dash.totalLivros;
        document.getElementById('dashTotalUsuarios').textContent = dash.totalUsuarios;
        document.getElementById('dashEmprestimosAtivos').textContent = dash.emprestimosAtivos;
        document.getElementById('dashLivrosAtrasados').textContent = dash.livrosAtrasados;

        // Gráfico de Categorias
        renderChartCategorias(dash.categoriasMaisEmprestadas);

        // Populares Gráfico
        const populares = await api.get('relatorios/populares');
        renderChartPopulares(populares);

        // Gráfico de Empréstimos por Mês
        if (dash.emprestimosPorMes && dash.emprestimosPorMes.length > 0) {
            renderChartEmprestimosMes(dash.emprestimosPorMes);
        }

        // Gráfico de Status e Atrasos
        if (dash.statusEmprestimos) {
            renderChartStatusAtrasos(dash.statusEmprestimos);
        }

        // Atrasados Tabela
        const atrasados = await api.get('relatorios/atrasados');
        renderAtrasados(atrasados);

    } catch (e) { console.error('Erro dashboard:', e); }
}

function renderChartCategorias(dados) {
    const ctx = document.getElementById('chartCategorias').getContext('2d');
    if(chartCategoriaInstance) chartCategoriaInstance.destroy();
    
    chartCategoriaInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: dados.map(d => d.categoria),
            datasets: [{
                data: dados.map(d => d.total),
                backgroundColor: ['#6366f1', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6'],
                borderWidth: 0
            }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: {
                legend: { position: 'right', labels: { color: '#f8fafc' } },
                title: { display: true, text: 'Empréstimos por Categoria', color: '#f8fafc', font: {size: 16} }
            }
        }
    });
}

function renderChartPopulares(dados) {
    const ctx = document.getElementById('chartPopulares').getContext('2d');
    if(chartPopularesInstance) chartPopularesInstance.destroy();
    
    chartPopularesInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: dados.map(d => d.titulo.substring(0, 15) + '...'),
            datasets: [{
                label: 'Total de Empréstimos',
                data: dados.map(d => d.totalEmprestimos),
                backgroundColor: '#6366f1',
                borderRadius: 4
            }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                title: { display: true, text: 'Livros Mais Populares', color: '#f8fafc', font: {size: 16} }
            },
            scales: {
                y: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(255,255,255,0.1)' } },
                x: { ticks: { color: '#94a3b8' }, grid: { display: false } }
            }
        }
    });
}

function renderChartEmprestimosMes(dados) {
    const ctx = document.getElementById('chartEmprestimosMes').getContext('2d');
    if(chartEmprestimosMesInstance) chartEmprestimosMesInstance.destroy();

    chartEmprestimosMesInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: dados.map(d => d.mes),
            datasets: [{
                label: 'Empréstimos Realizados',
                data: dados.map(d => d.total),
                borderColor: '#10b981',
                backgroundColor: 'rgba(16, 185, 129, 0.2)',
                fill: true,
                tension: 0.35,
                borderWidth: 2.5,
                pointBackgroundColor: '#10b981',
                pointRadius: 4
            }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                title: { display: true, text: 'Evolução de Empréstimos por Mês', color: '#f8fafc', font: {size: 16} }
            },
            scales: {
                y: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(255,255,255,0.1)' } },
                x: { ticks: { color: '#94a3b8' }, grid: { display: false } }
            }
        }
    });
}

function renderChartStatusAtrasos(status) {
    const ctx = document.getElementById('chartStatusAtrasos').getContext('2d');
    if(chartStatusAtrasosInstance) chartStatusAtrasosInstance.destroy();

    chartStatusAtrasosInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['No Prazo', 'Atrasados', 'Devolvidos'],
            datasets: [{
                data: [status.noPrazo, status.atrasados, status.devolvidos],
                backgroundColor: ['#10b981', '#ef4444', '#6366f1'],
                borderWidth: 0
            }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: {
                legend: { position: 'right', labels: { color: '#f8fafc' } },
                title: { display: true, text: 'Status de Empréstimos e Atrasos', color: '#f8fafc', font: {size: 16} }
            }
        }
    });
}

function renderAtrasados(atrasados) {
    const tbody = document.querySelector('#tabelaAtrasados tbody');
    tbody.innerHTML = '';
    atrasados.forEach(a => {
        tbody.innerHTML += `
            <tr>
                <td>${a.nomeAluno}<br><small style="color:var(--text-muted)">${a.emailAluno}</small></td>
                <td>${a.tituloLivro}</td>
                <td>${new Date(a.dataPrevistaDevolucao).toLocaleDateString()}</td>
                <td><span class="badge badge-danger">${a.diasAtraso} dias</span></td>
                <td><strong style="color:var(--danger)">R$ ${a.multa.toFixed(2)}</strong></td>
            </tr>`;
    });
}

// =================== LIVROS ===================
async function carregarLivros() {
    try {
        const termo = document.getElementById('buscaLivro').value;
        const res = await api.get(`livros`, { termo: termo, page: pageLivros, pageSize: 10 });
        const tbody = document.querySelector('#tabelaLivros tbody');
        tbody.innerHTML = '';
        
        res.itens.forEach(l => {
            tbody.innerHTML += `
                <tr>
                    <td>${l.isbn}</td>
                    <td><strong>${l.titulo}</strong><br><small style="color:var(--text-muted)">${l.editora} - ${l.anoPublicacao}</small></td>
                    <td>${l.nomeAutor}</td>
                    <td><span class="badge badge-primary">${l.categoria}</span></td>
                    <td>${l.quantidade > 0 ? `<span class="badge badge-success">${l.quantidade} un.</span>` : '<span class="badge badge-danger">Esgotado</span>'}</td>
                    <td>
                        <button class="btn btn-small" onclick="editarLivro(${l.id})">✏️</button>
                        <button class="btn btn-small btn-danger" onclick="excluirLivro(${l.id})">🗑️</button>
                    </td>
                </tr>`;
        });

        document.getElementById('infoPaginaLivros').textContent = `Página ${res.paginaAtual} de ${res.totalPaginas} (${res.totalItens} registros)`;
        document.getElementById('btnLivrosAnt').disabled = res.paginaAtual <= 1;
        document.getElementById('btnLivrosProx').disabled = res.paginaAtual >= res.totalPaginas;

    } catch (e) { alert(e.message); }
}

async function carregarAutoresSelect() {
    try {
        const autores = await api.get('autores');
        const select = document.getElementById('livroAutor');
        select.innerHTML = '<option value="">Selecione um autor...</option>';
        autores.forEach(a => {
            select.innerHTML += `<option value="${a.id}">${a.nome}</option>`;
        });
    } catch(e) {}
}

function abrirModalLivro() {
    document.getElementById('formLivro').reset();
    document.getElementById('livroId').value = '';
    document.getElementById('modalLivroTitulo').textContent = 'Novo Livro';
    document.getElementById('modalLivro').showModal();
}

async function salvarLivro() {
    const id = document.getElementById('livroId').value;
    const dto = {
        isbn: document.getElementById('livroIsbn').value,
        titulo: document.getElementById('livroTitulo').value,
        descricao: document.getElementById('livroDescricao').value,
        anoPublicacao: parseInt(document.getElementById('livroAno').value),
        editora: document.getElementById('livroEditora').value,
        categoria: document.getElementById('livroCategoria').value,
        quantidade: parseInt(document.getElementById('livroQtd').value),
        localizacao: document.getElementById('livroLocalizacao').value,
        autorId: parseInt(document.getElementById('livroAutor').value)
    };

    try {
        if (id) await api.put(`livros/${id}`, dto);
        else await api.post('livros', dto);
        
        document.getElementById('modalLivro').close();
        carregarLivros();
    } catch (e) { alert(e.message); }
}

async function editarLivro(id) {
    try {
        const livro = await api.get(`livros/${id}`);
        document.getElementById('livroId').value = livro.id;
        document.getElementById('livroIsbn').value = livro.isbn;
        document.getElementById('livroTitulo').value = livro.titulo;
        document.getElementById('livroDescricao').value = livro.descricao;
        document.getElementById('livroAno').value = livro.anoPublicacao;
        document.getElementById('livroEditora').value = livro.editora;
        document.getElementById('livroCategoria').value = livro.categoria;
        document.getElementById('livroQtd').value = livro.quantidade;
        document.getElementById('livroLocalizacao').value = livro.localizacao;
        document.getElementById('livroAutor').value = livro.autorId;
        
        document.getElementById('modalLivroTitulo').textContent = 'Editar Livro';
        document.getElementById('modalLivro').showModal();
    } catch (e) { alert(e.message); }
}

async function excluirLivro(id) {
    if(!confirm('Tem certeza?')) return;
    try {
        await api.delete(`livros/${id}`);
        carregarLivros();
    } catch (e) { alert(e.message); }
}

// =================== AUTORES, ALUNOS ===================

function abrirModalAutor() {
    document.getElementById('formAutor').reset();
    document.getElementById('autorId').value = '';
    document.getElementById('modalAutorTitulo').textContent = 'Novo Autor';
    document.getElementById('modalAutor').showModal();
}

function editarAutor(a) {
    document.getElementById('formAutor').reset();
    document.getElementById('autorId').value = a.id;
    document.getElementById('modalAutorTitulo').textContent = 'Editar Autor';
    document.getElementById('autorNome').value = a.nome;
    document.getElementById('autorNac').value = a.nacionalidade;
    if (a.dataNascimento) {
        document.getElementById('autorData').value = a.dataNascimento.split('T')[0];
    }
    document.getElementById('modalAutor').showModal();
}

async function excluirAutor(id) {
    if (!confirm('Deseja realmente excluir este autor?')) return;
    try {
        await api.delete(`autores/${id}`);
        carregarAutores();
        carregarAutoresSelect();
    } catch (e) {
        alert('Erro ao excluir autor: ' + e.message);
    }
}

async function salvarAutor() {
    const id = document.getElementById('autorId').value;
    const rawData = document.getElementById('autorData').value;
    const dataNascimentoIso = rawData ? new Date(rawData + 'T12:00:00Z').toISOString() : new Date().toISOString();

    const dto = {
        nome: document.getElementById('autorNome').value.trim(),
        nacionalidade: document.getElementById('autorNac').value.trim(),
        dataNascimento: dataNascimentoIso
    };

    try {
        if (id) await api.put(`autores/${id}`, dto);
        else await api.post('autores', dto);
        
        document.getElementById('modalAutor').close();
        carregarAutores();
        carregarAutoresSelect();
    } catch (e) { 
        alert('Erro ao salvar autor: ' + e.message); 
    }
}

async function carregarAutores() {
    try {
        const autores = await api.get('autores');
        const tbody = document.querySelector('#tabelaAutores tbody');
        tbody.innerHTML = '';
        if (!autores || autores.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" style="text-align:center; color:var(--text-muted)">Nenhum autor cadastrado.</td></tr>';
            return;
        }
        autores.forEach(a => {
            const dataFmt = a.dataNascimento ? new Date(a.dataNascimento).toLocaleDateString() : '-';
            const autorJson = JSON.stringify(a).replace(/'/g, "&apos;");
            tbody.innerHTML += `
                <tr>
                    <td>${a.id}</td>
                    <td><strong>${a.nome}</strong></td>
                    <td>${a.nacionalidade}</td>
                    <td>${dataFmt}</td>
                    <td>
                        <button class="btn btn-small" onclick='editarAutor(${autorJson})'>✏️</button>
                        <button class="btn btn-small btn-danger" onclick="excluirAutor(${a.id})">🗑️</button>
                    </td>
                </tr>`;
        });
    } catch(e) {
        console.error('Erro ao carregar autores:', e);
    }
}

function abrirModalAluno() {
    document.getElementById('formAluno').reset();
    document.getElementById('alunoId').value = '';
    document.getElementById('modalAlunoTitulo').textContent = 'Novo Aluno';
    document.getElementById('modalAluno').showModal();
}

function editarAluno(a) {
    document.getElementById('formAluno').reset();
    document.getElementById('alunoId').value = a.id;
    document.getElementById('modalAlunoTitulo').textContent = 'Editar Aluno';
    document.getElementById('alunoNome').value = a.nome;
    document.getElementById('alunoMatricula').value = a.matricula;
    document.getElementById('alunoEmail').value = a.email;
    document.getElementById('modalAluno').showModal();
}

async function excluirAluno(id) {
    const user = auth.getUser();
    if (user && user.perfil !== 'Admin') {
        alert('Apenas administradores possuem permissão para excluir alunos.');
        return;
    }
    if (!confirm('Deseja realmente excluir este aluno? Esta ação não pode ser desfeita.')) return;
    try {
        await api.delete(`alunos/${id}`);
        carregarAlunos();
    } catch (e) {
        alert('Erro ao excluir aluno: ' + e.message);
    }
}

async function salvarAluno() {
    const id = document.getElementById('alunoId').value;
    const dto = {
        nome: document.getElementById('alunoNome').value.trim(),
        matricula: document.getElementById('alunoMatricula').value.trim(),
        email: document.getElementById('alunoEmail').value.trim()
    };

    if (!dto.nome || !dto.matricula || !dto.email) {
        alert('Por favor, preencha todos os campos obrigatórios.');
        return;
    }

    try {
        if (id) await api.put(`alunos/${id}`, dto);
        else await api.post('alunos', dto);
        
        document.getElementById('modalAluno').close();
        carregarAlunos();
    } catch (e) { 
        alert('Erro ao salvar aluno: ' + e.message); 
    }
}

async function carregarAlunos() {
    try {
        const alunos = await api.get('alunos');
        const tbody = document.querySelector('#tabelaAlunos tbody');
        tbody.innerHTML = '';
        if (!alunos || alunos.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align:center; color:var(--text-muted)">Nenhum aluno cadastrado.</td></tr>';
            return;
        }

        const user = auth.getUser();
        const isAdmin = user && user.perfil === 'Admin';

        alunos.forEach(a => {
            const alunoJson = JSON.stringify(a).replace(/'/g, "&apos;");
            tbody.innerHTML += `
                <tr>
                    <td><strong>${a.matricula}</strong></td>
                    <td>${a.nome}</td>
                    <td>${a.email}</td>
                    <td>
                        <button class="btn btn-small" onclick='editarAluno(${alunoJson})' title="Editar Aluno">✏️</button>
                        ${isAdmin ? `<button class="btn btn-small btn-danger" onclick="excluirAluno(${a.id})" title="Excluir Aluno">🗑️</button>` : ''}
                    </td>
                </tr>`;
        });
    } catch(e) {
        console.error('Erro ao carregar alunos:', e);
    }
}

// =================== EMPRÉSTIMOS ===================
async function abrirModalEmprestimo() {
    document.getElementById('formEmprestimo').reset();
    try {
        const alunos = await api.get('alunos');
        const selAluno = document.getElementById('emprestimoAluno');
        selAluno.innerHTML = '<option value="">Selecione um aluno...</option>';
        alunos.forEach(a => selAluno.innerHTML += `<option value="${a.id}">${a.nome} (${a.matricula})</option>`);

        const livros = await api.get('livros', { pageSize: 100 });
        const selLivro = document.getElementById('emprestimoLivro');
        selLivro.innerHTML = '<option value="">Selecione um livro (com estoque)...</option>';
        livros.itens.forEach(l => {
            if (l.quantidade > 0) {
                selLivro.innerHTML += `<option value="${l.id}">${l.titulo}</option>`;
            }
        });

        document.getElementById('modalEmprestimo').showModal();
    } catch (e) { alert('Erro ao carregar dados: ' + e.message); }
}

async function salvarEmprestimo() {
    const dto = {
        alunoId: parseInt(document.getElementById('emprestimoAluno').value),
        livroId: parseInt(document.getElementById('emprestimoLivro').value),
        dataPrevistaDevolucao: document.getElementById('emprestimoData').value
    };

    try {
        await api.post('emprestimos', dto);
        document.getElementById('modalEmprestimo').close();
        carregarEmprestimos();
    } catch (e) { alert(e.message); }
}

async function carregarEmprestimos(apenasAbertos = false) {
    try {
        const endpoint = apenasAbertos ? 'emprestimos/abertos' : 'emprestimos';
        const emprestimos = await api.get(endpoint);
        const tbody = document.querySelector('#tabelaEmprestimos tbody');
        tbody.innerHTML = '';
        
        emprestimos.forEach(e => {
            const statusBadge = e.status === 'Ativo' ? 
                (e.diasAtraso > 0 ? `<span class="badge badge-danger">Atrasado (${e.diasAtraso}d)</span>` : '<span class="badge badge-warning">Ativo</span>') : 
                '<span class="badge badge-success">Devolvido</span>';
            
            const multaStr = e.multa > 0 ? `<br><small style="color:var(--danger)">Multa: R$ ${e.multa.toFixed(2)}</small>` : '';
            const devolucaoStr = e.dataDevolucao ? new Date(e.dataDevolucao).toLocaleDateString() : '-';

            tbody.innerHTML += `
                <tr>
                    <td>${e.id}</td>
                    <td>${e.nomeAluno}</td>
                    <td>${e.tituloLivro}</td>
                    <td>${new Date(e.dataEmprestimo).toLocaleDateString()}</td>
                    <td>${devolucaoStr}</td>
                    <td>${statusBadge} ${multaStr}</td>
                    <td>
                        ${e.status === 'Ativo' ? `<button class="btn btn-small btn-primary" onclick="devolver(${e.id})">Devolver</button>` : ''}
                    </td>
                </tr>`;
        });
    } catch(e){}
}

async function devolver(id) {
    if(!confirm('Confirmar devolução?')) return;
    try {
        await api.put(`emprestimos/${id}/devolucao`);
        carregarEmprestimos();
    } catch(e) { alert(e.message); }
}

// =================== RESERVAS ===================
async function carregarSelectLivrosParaReserva() {
    try {
        const res = await api.get('livros', { pageSize: 100 });
        const select = document.getElementById('selectLivroReserva');
        select.innerHTML = '<option value="">Selecione um livro...</option>';
        res.itens.forEach(l => {
            select.innerHTML += `<option value="${l.id}">${l.titulo} (Estoque: ${l.quantidade})</option>`;
        });
    } catch(e){}
}

async function carregarFilaReserva() {
    const livroId = document.getElementById('selectLivroReserva').value;
    if(!livroId) {
        document.querySelector('#tabelaReservas tbody').innerHTML = '';
        return;
    }
    
    try {
        const fila = await api.get(`reservas/fila/${livroId}`);
        const tbody = document.querySelector('#tabelaReservas tbody');
        tbody.innerHTML = '';
        
        if (fila.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align:center">Nenhuma reserva ativa para este livro.</td></tr>';
            return;
        }

        fila.forEach((r, index) => {
            tbody.innerHTML += `
                <tr>
                    <td><span class="badge badge-primary">#${index + 1}</span></td>
                    <td>${r.nomeAluno}</td>
                    <td>${new Date(r.dataReserva).toLocaleString()}</td>
                    <td><span class="badge badge-warning">${r.status}</span></td>
                </tr>`;
        });
    } catch(e){ alert(e.message); }
}

// =================== AUDITORIA ===================
async function carregarAuditoria() {
    try {
        const res = await api.get('auditoria', { page: pageAuditoria, pageSize: 20 });
        const tbody = document.querySelector('#tabelaAuditoria tbody');
        tbody.innerHTML = '';
        
        res.itens.forEach(log => {
            let actionBadge = 'badge-primary';
            if(log.acao === 'Excluiu') actionBadge = 'badge-danger';
            if(log.acao === 'Atualizou') actionBadge = 'badge-warning';

            tbody.innerHTML += `
                <tr>
                    <td><small style="color:var(--text-muted)">${new Date(log.dataHora).toLocaleString()}</small></td>
                    <td><strong>${log.nomeUsuario}</strong></td>
                    <td><span class="badge ${actionBadge}">${log.acao}</span></td>
                    <td>${log.entidade} #${log.entidadeId}</td>
                    <td><small>${log.detalhes}</small></td>
                </tr>`;
        });

        document.getElementById('infoPaginaAuditoria').textContent = `Página ${res.paginaAtual} de ${res.totalPaginas} (${res.totalItens} registros)`;
        document.getElementById('btnAuditoriaAnt').disabled = res.paginaAtual <= 1;
        document.getElementById('btnAuditoriaProx').disabled = res.paginaAtual >= res.totalPaginas;

    } catch(e) {}
}
