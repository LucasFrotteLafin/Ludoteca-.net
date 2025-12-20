using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ludoteca.Models;

namespace Ludoteca.Services;

public class BibliotecaJogos
{
    private List<Jogo> jogos;
    private List<Membro> membros;
    private List<Emprestimo> emprestimos;
    private int proximoIdJogo;
    private int proximoIdEmprestimo = 1;
    private readonly string caminhoArquivo;

    public BibliotecaJogos(string? caminhoPersonalizado = null)
    {
        jogos = new List<Jogo>();
        membros = new List<Membro>();
        emprestimos = new List<Emprestimo>();
        proximoIdJogo = 1;
        caminhoArquivo = caminhoPersonalizado ?? "Data/biblioteca.json";

        try
        {
            VerificarPasta();
            CarregarDados();
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR - Inicialização: {ex.Message} | StackTrace: {ex.StackTrace}\n");
        }
    }

    private void VerificarPasta()
    {
        string pasta = "Data";
        if (!Directory.Exists(pasta))
        {
            Directory.CreateDirectory(pasta);
        }
    }

    private int GerarProximoIdEmprestimo()
    {
        return proximoIdEmprestimo++;
    }

    public void AdicionarJogo(string nome, string categoria, int idadeMinima)
    {
        if (jogos == null)
            throw new InvalidOperationException("Lista de jogos não inicializada");

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do jogo não pode estar vazio");

        if (string.IsNullOrWhiteSpace(categoria))
            throw new ArgumentException("Categoria não pode estar vazia");

        // Verificar se jogo já existe (case-insensitive)
        bool jogoExiste = jogos.Any(j => string.Equals(j.Nome.Trim(), nome.Trim(), StringComparison.OrdinalIgnoreCase));

        if (jogoExiste)
        {
            throw new ArgumentException("Jogo já existe!");
        }

        // Criar e adicionar novo jogo
        Jogo novoJogo = new Jogo(proximoIdJogo, nome.Trim(), categoria.Trim(), idadeMinima);
        jogos.Add(novoJogo);
        if (proximoIdJogo >= int.MaxValue)
            throw new InvalidOperationException("Limite máximo de jogos atingido");
        proximoIdJogo++;
        SalvarDados();
    }

    public void AdicionarMembro(string nome, string email, string telefone, int codigoMembro, DateTime dataNascimento)
    {
        if (membros == null)
            throw new InvalidOperationException("Lista de membros não inicializada");

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode estar vazio");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode estar vazio");

        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone não pode estar vazio");

        // Verificar se email já existe (case-insensitive)
        bool emailExiste = membros.Any(m => string.Equals(m.Email.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (emailExiste)
        {
            throw new ArgumentException("Email já existe!");
        }

        // Verificar se código já existe
        bool codigoExiste = membros.Any(m => m.CodigoMembro == codigoMembro);

        if (codigoExiste)
        {
            throw new ArgumentException("Código do membro já existe!");
        }

        // Criar e adicionar novo membro
        Membro novoMembro = new Membro(codigoMembro, nome.Trim(), email.Trim(), telefone.Trim(), dataNascimento);
        membros.Add(novoMembro);
        SalvarDados();
    }

    public void RealizarEmprestimo(int idJogo, int codigoMembro)
    {
        if (jogos == null || membros == null || emprestimos == null)
            throw new InvalidOperationException("Listas não inicializadas");

        // Buscar jogo
        Jogo? jogo = jogos.FirstOrDefault(j => j.Id == idJogo);

        if (jogo == null)
        {
            throw new ArgumentException("Jogo não encontrado!");
        }

        // Buscar membro
        Membro? membro = membros.FirstOrDefault(m => m.CodigoMembro == codigoMembro);

        if (membro == null)
        {
            throw new ArgumentException($"Código do membro {codigoMembro} não encontrado!");
        }

        // Verificar se membro já tem empréstimo ativo
        bool membroTemEmprestimoAtivo = emprestimos.Any(e => e.CodigoMembro == codigoMembro && e.Ativo);
        if (membroTemEmprestimoAtivo)
        {
            throw new InvalidOperationException("Membro já possui um empréstimo ativo!");
        }

        // Verificar idade mínima
        if (!membro.PodeAlugarJogo(jogo.IdadeMinima))
        {
            throw new InvalidOperationException($"Membro tem {membro.Idade} anos. Idade mínima para este jogo: {jogo.IdadeMinima} anos");
        }

        if (jogo.EstaEmprestado)
        {
            throw new InvalidOperationException("Jogo já está emprestado!");
        }

        // Realizar empréstimo
        jogo.MarcarComoEmprestado();
        Emprestimo novoEmprestimo = new Emprestimo(idJogo, codigoMembro, 7)
        {
            Id = GerarProximoIdEmprestimo()
        };
        emprestimos.Add(novoEmprestimo);
        SalvarDados();
    }

    public void RealizarDevolucao(int idJogo)
    {
        if (jogos == null || emprestimos == null)
            throw new InvalidOperationException("Listas não inicializadas");

        Jogo? jogo = jogos.FirstOrDefault(j => j.Id == idJogo);

        if (jogo == null)
        {
            throw new ArgumentException("Jogo não encontrado!", nameof(idJogo));
        }

        if (!jogo.EstaEmprestado)
        {
            throw new InvalidOperationException("Jogo não está emprestado!");
        }
        
        // Buscar o empréstimo ativo mais recente para este jogo
        Emprestimo? emprestimo = null;
        for (int i = emprestimos.Count - 1; i >= 0; i--)
        {
            if (emprestimos[i].IdJogo == idJogo && emprestimos[i].Ativo)
            {
                emprestimo = emprestimos[i];
                break;
            }
        }

        if (emprestimo == null)
        {
            throw new InvalidOperationException("Empréstimo não encontrado para este jogo!");
        }

        if (emprestimo.EstaAtrasado())
        {
            Console.WriteLine($"Jogo está atrasado! Multa: R$ {emprestimo.ValorMulta:F2}");
            Console.Write("Deseja pagar a multa agora? (s/n): ");
            string resposta = Console.ReadLine()?.ToLower() ?? "";

            if (resposta == "s" || resposta == "sim")
            {
                Console.Write("Forma de pagamento (pix/dinheiro): ");
                string formaPagamento = Console.ReadLine() ?? "";

                if (formaPagamento.ToLower() == "pix" || formaPagamento.ToLower() == "dinheiro")
                {
                    emprestimo.RegistrarPagamentoMulta(formaPagamento.ToLower());
                    Console.WriteLine($"Multa de R$ {emprestimo.ValorMulta:F2} ({emprestimo.DiasAtraso} dias excedidos) paga via {formaPagamento}.");
                    
                    // Registrar devolução
                    emprestimo.RegistrarDevolucao();
                    jogo.MarcarComoDevolvido();
                    SalvarDados();
                    Console.WriteLine("Jogo devolvido com sucesso!");
                }
                else
                {
                    Console.WriteLine("Forma de pagamento inválida. Use apenas 'pix' ou 'dinheiro'.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Multa não paga. Jogo não foi devolvido. Você deve pagar a multa antes de devolver.");
                return;
            }
        }
        else
        {
            // Registrar devolução
            emprestimo.RegistrarDevolucao();
            jogo.MarcarComoDevolvido();
            SalvarDados();
            Console.WriteLine("Jogo devolvido com sucesso!");
        }
    }

    public void SalvarDados()
    {
        try
        {
            VerificarPasta();

            DadosBiblioteca dados = new DadosBiblioteca();
            dados.Jogos = jogos;
            dados.Membros = membros;
            dados.Emprestimos = emprestimos;
            dados.ProximoIdJogo = proximoIdJogo;
            dados.ProximoIdEmprestimo = proximoIdEmprestimo;

            JsonSerializerOptions opcoes = new JsonSerializerOptions();
            opcoes.WriteIndented = true;
            opcoes.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            string json = JsonSerializer.Serialize(dados, opcoes);
            File.WriteAllText(caminhoArquivo, json);
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR - SalvarDados: {ex.Message} | StackTrace: {ex.StackTrace}\n");
            Console.WriteLine($"Erro ao salvar dados: {ex.Message}");
        }
    }

    public void CarregarDados()
    {
        try
        {
            // Limpar listas antes de carregar novos dados
            jogos.Clear();
            membros.Clear();
            emprestimos.Clear();
            
            if (!File.Exists(caminhoArquivo))
            {
                return;
            }

            string json = File.ReadAllText(caminhoArquivo);

            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            JsonSerializerOptions opcoes = new JsonSerializerOptions();
            opcoes.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            DadosBiblioteca? dados = JsonSerializer.Deserialize<DadosBiblioteca>(json, opcoes);

            if (dados != null)
            {
                jogos.AddRange(dados.Jogos ?? new List<Jogo>());
                membros.AddRange(dados.Membros ?? new List<Membro>());
                emprestimos.AddRange(dados.Emprestimos ?? new List<Emprestimo>());
                proximoIdJogo = dados.ProximoIdJogo;
                proximoIdEmprestimo = dados.ProximoIdEmprestimo > 0 ? dados.ProximoIdEmprestimo : 1;
            }
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR - CarregarDados: {ex.Message} | StackTrace: {ex.StackTrace}\n");
            Console.WriteLine($"Erro ao carregar dados: {ex.Message}");
        }
    }

    public decimal CalcularMulta(int idJogo)
    {
        // Não recarregar dados aqui - usar dados em memória
        Jogo? jogo = jogos.FirstOrDefault(j => j.Id == idJogo);
        
        if (jogo == null)
        {
            Console.WriteLine($"Jogo ID {idJogo} não encontrado");
            return 0;
        }
        
        if (!jogo.EstaEmprestado)
        {
            Console.WriteLine($"Jogo '{jogo.Nome}' não está emprestado");
            return 0;
        }
        
        // Buscar o empréstimo ativo mais recente para este jogo
        Emprestimo? emprestimoAtual = null;
        for (int i = emprestimos.Count - 1; i >= 0; i--)
        {
            if (emprestimos[i].IdJogo == idJogo && emprestimos[i].Ativo)
            {
                emprestimoAtual = emprestimos[i];
                break;
            }
        }
        
        if (emprestimoAtual == null)
        {
            Console.WriteLine($"Jogo '{jogo.Nome}' marcado como emprestado mas sem empréstimo registrado");
            return 0;
        }
        
        if (emprestimoAtual.EstaAtrasado())
        {
            Console.WriteLine($"Jogo '{jogo.Nome}' emprestado em {emprestimoAtual.DataEmprestimo:dd/MM/yyyy}, deveria ter sido devolvido em {emprestimoAtual.DataDevolucao:dd/MM/yyyy}. Atraso: {emprestimoAtual.DiasAtraso} dias. Multa: R$ {emprestimoAtual.ValorMulta:F2}");
            return emprestimoAtual.ValorMulta;
        }
        
        Console.WriteLine($"{jogo.Nome} está dentro do prazo registrado");
        return 0;
    }

    public void VerificarMulta()
    {
        // Não recarregar dados aqui - usar dados em memória
        int idJogo;
        while (true)
        {
            Console.Write("ID do jogo para verificar multa: ");
            string input = Console.ReadLine() ?? "";
            
            // Validar se contém apenas números
            if (!System.Text.RegularExpressions.Regex.IsMatch(input, @"^[0-9]+$"))
            {
                Console.WriteLine("Erro: ID deve conter apenas números. Tente novamente.");
                continue;
            }
            
            // Validar tamanho do ID
            if (input.Length > 9)
            {
                Console.WriteLine("Erro: ID muito longo (máximo 9 dígitos). Tente novamente.");
                continue;
            }
            
            if (int.TryParse(input, out idJogo) && idJogo > 0)
                break;
            Console.WriteLine("Erro: Digite um número válido maior que zero.");
        }

        decimal multa = CalcularMulta(idJogo);
        Console.WriteLine($"\nResultado: R$ {multa:F2}");
    }

    public void CadastrarJogo()
    {
        try
        {
            Console.Write("Nome do jogo: ");
            string nome = Console.ReadLine()?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(nome))
            {
                Console.WriteLine("Erro: Nome do jogo não pode estar vazio");
                return;
            }
            
            // Validar se nome contém apenas letras e espaços
            if (!System.Text.RegularExpressions.Regex.IsMatch(nome, @"^[a-zA-ZÀ-ÿ\s]+$"))
            {
                Console.WriteLine("Erro: Nome deve conter apenas letras e espaços");
                return;
            }
            
            // Validar tamanho do nome
            if (nome.Length > 100)
            {
                Console.WriteLine("Erro: Nome não pode ter mais de 100 caracteres");
                return;
            }
            
            Console.Write("Categoria: ");
            string categoria = Console.ReadLine()?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(categoria))
            {
                Console.WriteLine("Erro: Categoria não pode estar vazia");
                return;
            }
            
            // Validar se categoria contém apenas letras e espaços
            if (!System.Text.RegularExpressions.Regex.IsMatch(categoria, @"^[a-zA-ZÀ-ÿ\s]+$"))
            {
                Console.WriteLine("Erro: Categoria deve conter apenas letras e espaços");
                return;
            }
            
            // Validar tamanho da categoria
            if (categoria.Length > 50)
            {
                Console.WriteLine("Erro: Categoria não pode ter mais de 50 caracteres");
                return;
            }
            Console.Write("Idade mínima: ");
            string idadeInput = Console.ReadLine() ?? "";
            
            // Validar se idade contém apenas números
            if (!System.Text.RegularExpressions.Regex.IsMatch(idadeInput, @"^[0-9]+$"))
            {
                Console.WriteLine("Erro: Idade deve conter apenas números");
                return;
            }

            if (!int.TryParse(idadeInput, out int idade))
            {
                Console.WriteLine("Erro: Idade deve ser um número válido");
                return;
            }
            
            // Validar faixa de idade
            if (idade < 0 || idade > 18)
            {
                Console.WriteLine("Erro: Idade deve estar entre 0 e 18 anos");
                return;
            }

            AdicionarJogo(nome, categoria, idade);
            Console.WriteLine("Jogo cadastrado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }

    public void CadastrarMembro()
    {
        try
        {
            Console.Write("Nome: ");
            string nome = Console.ReadLine()?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(nome))
            {
                Console.WriteLine("Erro: Nome não pode estar vazio");
                return;
            }
            
            // Validar se nome contém apenas letras e espaços
            if (!System.Text.RegularExpressions.Regex.IsMatch(nome, @"^[a-zA-ZÀ-ÿ\s]+$"))
            {
                Console.WriteLine("Erro: Nome deve conter apenas letras e espaços");
                return;
            }
            
            Console.Write("Email: ");
            string email = Console.ReadLine()?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Erro: Email não pode estar vazio");
                return;
            }
            
            // Validar formato do email
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[a-zA-Z0-9._]+@[a-zA-Z0-9._]+\.[a-zA-Z]{2,}$"))
            {
                Console.WriteLine("Erro: Email deve ter formato válido (exemplo@dominio.com)");
                return;
            }
            
            // Validar tamanho do email
            if (email.Length > 100)
            {
                Console.WriteLine("Erro: Email não pode ter mais de 100 caracteres");
                return;
            }
            Console.Write("Telefone: ");
            string telefone = Console.ReadLine() ?? "";
            
            // Validar se telefone contém apenas números
            if (!System.Text.RegularExpressions.Regex.IsMatch(telefone, @"^[0-9]+$"))
            {
                Console.WriteLine("Erro: Telefone deve conter apenas números");
                return;
            }
            
            // Validar tamanho do telefone
            if (telefone.Length < 10 || telefone.Length > 11)
            {
                Console.WriteLine("Erro: Telefone deve ter 10 ou 11 dígitos");
                return;
            }
            Console.Write("Código do membro (número): ");
            string codigoInput = Console.ReadLine() ?? "";
            
            // Validar se código contém apenas números
            if (!System.Text.RegularExpressions.Regex.IsMatch(codigoInput, @"^[0-9]+$"))
            {
                Console.WriteLine("Erro: Código deve conter apenas números");
                return;
            }

            if (!int.TryParse(codigoInput, out int codigoMembro))
            {
                Console.WriteLine("Erro: Código deve ser um número válido");
                return;
            }
            
            // Validar faixa do código
            if (codigoMembro <= 0 || codigoMembro > 999999)
            {
                Console.WriteLine("Erro: Código deve estar entre 1 e 999999");
                return;
            }

            Console.Write("Data de nascimento (dd/MM/yyyy): ");
            string dataInput = Console.ReadLine() ?? "";
            
            // Validar se data contém apenas números e barras
            if (!System.Text.RegularExpressions.Regex.IsMatch(dataInput, @"^[0-9/]+$"))
            {
                Console.WriteLine("Erro: Data deve conter apenas números e barras (dd/MM/yyyy)");
                return;
            }
            
            if (!DateTime.TryParseExact(dataInput, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataNascimento))
            {
                Console.WriteLine("Erro: Data deve estar no formato dd/MM/yyyy");
                return;
            }
            
            // Validar se data não é futura
            if (dataNascimento > DateTime.Now)
            {
                Console.WriteLine("Erro: Data de nascimento não pode ser futura");
                return;
            }
            
            // Validar idade máxima (120 anos)
            if (dataNascimento < DateTime.Now.AddYears(-120))
            {
                Console.WriteLine("Erro: Idade não pode ser superior a 120 anos");
                return;
            }

            AdicionarMembro(nome, email, telefone, codigoMembro, dataNascimento);
            Console.WriteLine("Membro cadastrado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }

    public void ListarJogos()
    {
        if (jogos.Count == 0)
        {
            Console.WriteLine("Nenhum jogo cadastrado.");
            return;
        }

        Console.WriteLine("\n=== JOGOS CADASTRADOS ===");
        foreach (Jogo jogo in jogos)
        {
            Console.WriteLine(jogo.ToString());
        }
    }

    public void EmprestarJogo()
    {
        try
        {
            Console.Write("ID do jogo: ");
            string idInput = Console.ReadLine() ?? "";
            
            // Validar se ID contém apenas números
            if (!System.Text.RegularExpressions.Regex.IsMatch(idInput, @"^[0-9]+$"))
            {
                Console.WriteLine("Erro: ID deve conter apenas números");
                return;
            }
            
            if (!int.TryParse(idInput, out int idJogo))
            {
                Console.WriteLine("Erro: ID do jogo deve ser um número válido");
                return;
            }
            
            if (idJogo <= 0)
            {
                Console.WriteLine("Erro: ID do jogo deve ser maior que zero");
                return;
            }

            Console.Write("Código do membro: ");
            string codigoInput = Console.ReadLine() ?? "";
            
            // Validar se código contém apenas números
            if (!System.Text.RegularExpressions.Regex.IsMatch(codigoInput, @"^[0-9]+$"))
            {
                Console.WriteLine("Erro: Código deve conter apenas números");
                return;
            }
            
            if (!int.TryParse(codigoInput, out int codigoMembro))
            {
                Console.WriteLine("Erro: Código do membro deve ser um número válido");
                return;
            }
            
            if (codigoMembro <= 0)
            {
                Console.WriteLine("Erro: Código do membro deve ser maior que zero");
                return;
            }

            RealizarEmprestimo(idJogo, codigoMembro);
            Console.WriteLine("Jogo emprestado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }

    public void DevolverJogo()
    {
        try
        {
            Console.Write("ID do jogo: ");
            string idInput = Console.ReadLine() ?? "";
            
            // Validar se ID contém apenas números
            if (!System.Text.RegularExpressions.Regex.IsMatch(idInput, @"^[0-9]+$"))
            {
                Console.WriteLine("Erro: ID deve conter apenas números");
                return;
            }
            
            if (!int.TryParse(idInput, out int idJogo))
            {
                Console.WriteLine("Erro: ID do jogo deve ser um número válido");
                return;
            }
            
            if (idJogo <= 0)
            {
                Console.WriteLine("Erro: ID do jogo deve ser maior que zero");
                return;
            }

            RealizarDevolucao(idJogo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }

    public void GerarRelatorio()
    {
        try
        {
            VerificarPasta();

            string caminhoRelatorio = Path.Combine("Data", "relatorio.txt");

            string relatorio = "\n=== RELATÓRIO DA LUDOTECA ===\n";
            relatorio += $"Data: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n";

            relatorio += "JOGOS CADASTRADOS:\n";
            for (int i = 0; i < jogos.Count; i++)
            {
                relatorio += jogos[i].ToString() + "\n";
            }

            relatorio += "\nMEMBROS CADASTRADOS:\n";
            for (int i = 0; i < membros.Count; i++)
            {
                relatorio += membros[i].ToString() + "\n";
            }

            relatorio += "\nJOGOS EMPRESTADOS:\n";
            for (int i = 0; i < jogos.Count; i++)
            {
                if (jogos[i].EstaEmprestado)
                {
                    // Buscar empréstimo ativo mais recente
                    Emprestimo? emprestimo = null;
                    for (int j = emprestimos.Count - 1; j >= 0; j--)
                    {
                        if (emprestimos[j].IdJogo == jogos[i].Id && emprestimos[j].Ativo)
                        {
                            emprestimo = emprestimos[j];
                            break;
                        }
                    }
                    
                    if (emprestimo != null)
                    {
                        string statusMulta = "";
                        if (emprestimo.EstaAtrasado())
                        {
                            statusMulta = $" | ATRASADO: {emprestimo.DiasAtraso} dias - Multa: R$ {emprestimo.ValorMulta:F2}";
                        }

                        relatorio += $"Jogo: {jogos[i].Nome} | Membro: {emprestimo.CodigoMembro} | Empréstimo: {emprestimo.DataEmprestimo:dd/MM/yyyy} | Devolução: {emprestimo.DataDevolucao:dd/MM/yyyy}{statusMulta}\n";
                    }
                }
            }

            relatorio += "\nHISTÓRICO DE DEVOLUÇÕES:\n";
            for (int i = 0; i < emprestimos.Count; i++)
            {
                if (emprestimos[i].DataDevolucaoReal.HasValue)
                {
                    string nomeJogo = "Jogo não encontrado";
                    for (int j = 0; j < jogos.Count; j++)
                    {
                        if (jogos[j].Id == emprestimos[i].IdJogo)
                        {
                            nomeJogo = jogos[j].Nome;
                            break;
                        }
                    }

                    string infoMulta = "";
                    if (emprestimos[i].MultaPaga)
                    {
                        infoMulta = $" | MULTA PAGA: R$ {emprestimos[i].ValorMulta:F2} - {emprestimos[i].MetodoPagamento}";
                    }

                    relatorio += $"Jogo: {nomeJogo} | Membro: {emprestimos[i].CodigoMembro} | Devolvido em: {emprestimos[i].DataDevolucaoReal:dd/MM/yyyy HH:mm}{infoMulta}\n";
                }
            }

            relatorio += "\nHISTÓRICO DE EMPRÉSTIMOS:\n";
            for (int i = 0; i < emprestimos.Count; i++)
            {
                string nomeJogo = "Jogo não encontrado";
                for (int j = 0; j < jogos.Count; j++)
                {
                    if (jogos[j].Id == emprestimos[i].IdJogo)
                    {
                        nomeJogo = jogos[j].Nome;
                        break;
                    }
                }

                string infoMulta = "";
                if (emprestimos[i].MultaPaga)
                {
                    infoMulta = $" | MULTA PAGA: R$ {emprestimos[i].ValorMulta:F2} ({emprestimos[i].DiasAtraso} dias) - {emprestimos[i].MetodoPagamento}";
                }

                string statusDevolucao = emprestimos[i].DataDevolucaoReal.HasValue ? 
                    $" | DEVOLVIDO: {emprestimos[i].DataDevolucaoReal:dd/MM/yyyy HH:mm}" : " | EM ANDAMENTO";

                relatorio += $"Jogo: {nomeJogo} | Membro: {emprestimos[i].CodigoMembro} | Empréstimo: {emprestimos[i].DataEmprestimo:dd/MM/yyyy} | Devolução: {emprestimos[i].DataDevolucao:dd/MM/yyyy}{statusDevolucao}{infoMulta}\n";
            }

            relatorio += "\n" + new string('-', 50) + "\n";

            File.AppendAllText(caminhoRelatorio, relatorio);
            Console.WriteLine($"Relatório anexado em: {caminhoRelatorio}");
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR - GerarRelatorio: {ex.Message} | StackTrace: {ex.StackTrace}\n");
            Console.WriteLine($"Erro ao gerar relatório: {ex.Message}");
        }
    }



    public void PagarMulta(int idJogo, string formaPagamento)
    {
        decimal multa = CalcularMulta(idJogo);

        if (multa == 0)
        {
            Console.WriteLine("Não há multa para este jogo.");
            return;
        }

        string pagamento = formaPagamento.ToLower();
        if (string.Equals(pagamento, "pix", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(pagamento, "dinheiro", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Multa de R$ {multa:F2} paga via {formaPagamento}.");
        }
        else
        {
            Console.WriteLine("Forma de pagamento inválida. Use 'pix' ou 'dinheiro'.");
        }
    }

    public void ConsultarMulta()
    {
        Console.Write("ID do jogo: ");
        if (!int.TryParse(Console.ReadLine(), out int idJogo))
        {
            throw new ArgumentException("ID do jogo deve ser um número válido");
        }

        decimal multa = CalcularMulta(idJogo);
        if (multa > 0)
        {
            Console.WriteLine($"Multa: R$ {multa:F2}");
        }
        else
        {
            Console.WriteLine("Não há multa para este jogo.");
        }
    }

    public void ProcessarPagamentoMulta()
    {
        Console.Write("ID do jogo: ");
        if (!int.TryParse(Console.ReadLine(), out int idJogo))
        {
            throw new ArgumentException("ID do jogo deve ser um número válido");
        }

        Console.Write("Forma de pagamento (pix/dinheiro): ");
        string formaPagamento = Console.ReadLine() ?? "";

        PagarMulta(idJogo, formaPagamento);
    }

    public void RecarregarDados()
    {
        Console.WriteLine("\nAntes do recarregamento:");
        Console.WriteLine($"Jogos: {jogos.Count}, Membros: {membros.Count}, Empréstimos: {emprestimos.Count}");
        
        CarregarDados();
        
        Console.WriteLine("\nApós o recarregamento:");
        Console.WriteLine($"Jogos: {jogos.Count}, Membros: {membros.Count}, Empréstimos: {emprestimos.Count}");
        
        // Recalcular próximos IDs baseado nos dados carregados
        if (jogos.Count > 0)
        {
            proximoIdJogo = jogos.Max(j => j.Id) + 1;
        }
        else
        {
            proximoIdJogo = 1;
        }
        
        if (emprestimos.Count > 0)
        {
            proximoIdEmprestimo = emprestimos.Max(e => e.Id) + 1;
        }
        else
        {
            proximoIdEmprestimo = 1;
        }
        
        Console.WriteLine("\nDados recarregados do arquivo JSON!");
        Console.WriteLine($"Próximo ID de jogo: {proximoIdJogo}");
    }

}

public class DadosBiblioteca
{
    public List<Jogo> Jogos { get; set; } = new List<Jogo>();
    public List<Membro> Membros { get; set; } = new List<Membro>();
    public List<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
    public int ProximoIdJogo { get; set; }
    public int ProximoIdEmprestimo { get; set; } = 1;
}