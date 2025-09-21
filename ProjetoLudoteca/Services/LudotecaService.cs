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
    private int proximoIdMembro;
    private int proximoIdEmprestimo;

    private readonly string caminhoArquivo;
    private const int IDADE_MAXIMA = 18;

    public BibliotecaJogos(string? caminhoPersonalizado = null)
    {
        jogos = new List<Jogo>();
        membros = new List<Membro>();
        emprestimos = new List<Emprestimo>();
        proximoIdJogo = 1;
        proximoIdMembro = 1;
        proximoIdEmprestimo = 1;
        caminhoArquivo = caminhoPersonalizado ?? "data/biblioteca.json";

        VerificarPasta();
        CarregarDados();
    }

    private void VerificarPasta()
    {
        string pasta = "data";
        if (!Directory.Exists(pasta))
        {
            Directory.CreateDirectory(pasta);
        }
    }

    public void AdicionarJogo(string nome, string categoria, int idadeMinima)
    {
        // Verificar se jogo já existe
        bool jogoExiste = false;
        for (int i = 0; i < jogos.Count; i++)
        {
            if (jogos[i].Nome.ToLower() == nome.ToLower())
            {
                jogoExiste = true;
                break;
            }
        }

        if (jogoExiste)
        {
            throw new ArgumentException("Jogo já existe!");
        }

        Jogo novoJogo = new Jogo(proximoIdJogo, nome, categoria, idadeMinima);
        jogos.Add(novoJogo);
        proximoIdJogo++;
        SalvarDados();
    }

    public void AdicionarMembro(string nome, string email, string telefone, int codigoMembro)
    {
        // Verificar se email já existe
        bool emailExiste = false;
        for (int i = 0; i < membros.Count; i++)
        {
            if (membros[i].Email.ToLower() == email.ToLower())
            {
                emailExiste = true;
                break;
            }
        }

        if (emailExiste)
        {
            throw new ArgumentException("Email já existe!");
        }

        // Verificar se código já existe
        bool codigoExiste = false;
        for (int i = 0; i < membros.Count; i++)
        {
            if (membros[i].CodigoMembro == codigoMembro)
            {
                codigoExiste = true;
                break;
            }
        }

        if (codigoExiste)
        {
            throw new ArgumentException("Código do membro já existe!");
        }

        Membro novoMembro = new Membro(codigoMembro, nome, email, telefone);
        membros.Add(novoMembro);
        proximoIdMembro++;
        SalvarDados();
    }

    public bool AutenticarMembro(int codigoMembro)
    {
        for (int i = 0; i < membros.Count; i++)
        {
            if (membros[i].CodigoMembro == codigoMembro)
            {
                return true;
            }
        }
        return false;
    }

    public Membro? BuscarMembroPorCodigo(int codigoMembro)
    {
        for (int i = 0; i < membros.Count; i++)
        {
            if (membros[i].CodigoMembro == codigoMembro)
            {
                return membros[i];
            }
        }
        return null;
    }

    public void RealizarEmprestimo(int idJogo, int codigoMembro)
    {
        // Buscar jogo
        Jogo? jogo = null;
        for (int i = 0; i < jogos.Count; i++)
        {
            if (jogos[i].Id == idJogo)
            {
                jogo = jogos[i];
                break;
            }
        }

        if (jogo == null)
        {
            throw new ArgumentException("Jogo não encontrado!");
        }

        // Buscar membro pelo código
        Membro? membro = null;
        for (int i = 0; i < membros.Count; i++)
        {
            if (membros[i].CodigoMembro == codigoMembro)
            {
                membro = membros[i];
                break;
            }
        }

        if (membro == null)
        {
            throw new ArgumentException("Código do membro inválido!");
        }

        if (jogo.EstaEmprestado)
        {
            throw new InvalidOperationException("Jogo já está emprestado!");
        }

        jogo.MarcarComoEmprestado();
        Emprestimo novoEmprestimo = new Emprestimo(proximoIdEmprestimo, idJogo, codigoMembro, 7);
        emprestimos.Add(novoEmprestimo);
        proximoIdEmprestimo++;
        SalvarDados();
    }

    public void RealizarDevolucao(int idJogo)
    {
        // Buscar jogo
        Jogo? jogo = null;
        for (int i = 0; i < jogos.Count; i++)
        {
            if (jogos[i].Id == idJogo)
            {
                jogo = jogos[i];
                break;
            }
        }

        if (jogo == null)
        {
            throw new ArgumentException("Jogo não encontrado!", nameof(idJogo));
        }

        // Buscar empréstimo ativo
        Emprestimo? emprestimo = null;
        for (int i = 0; i < emprestimos.Count; i++)
        {
            if (emprestimos[i].IdJogo == idJogo && emprestimos[i].Ativo)
            {
                emprestimo = emprestimos[i];
                break;
            }
        }

        if (emprestimo == null)
        {
            throw new InvalidOperationException("Jogo não está emprestado!");
        }

        // if (emprestimo.EstaAtrasado())
        // {
        //     //LogInfo("Jogo está atrasado!");
        // }

        emprestimo.Devolver();
        jogo.MarcarComoDevolvido();
        SalvarDados();
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
            dados.ProximoIdMembro = proximoIdMembro;
            dados.ProximoIdEmprestimo = proximoIdEmprestimo;

            JsonSerializerOptions opcoes = new JsonSerializerOptions();
            opcoes.WriteIndented = true;
            opcoes.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            string json = JsonSerializer.Serialize(dados, opcoes); // [AV1-3]
            File.WriteAllText(caminhoArquivo, json);
        }
        // catch (Exception ex)
        // {
        //     //LogError($"Erro ao salvar dados: {ex.Message}");
        // }
    }

    public void CarregarDados()
    {
        try
        {
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

            DadosBiblioteca? dados = JsonSerializer.Deserialize<DadosBiblioteca>(json, opcoes); // [AV1-3]

            if (dados != null)
            {
                if (dados.Jogos != null)
                    jogos = dados.Jogos;

                if (dados.Membros != null)
                    membros = dados.Membros;

                if (dados.Emprestimos != null)
                    emprestimos = dados.Emprestimos;

                proximoIdJogo = dados.ProximoIdJogo;
                proximoIdMembro = dados.ProximoIdMembro;
                proximoIdEmprestimo = dados.ProximoIdEmprestimo;
            }
        }
        // catch (Exception ex)
        // {
        //     //LogError($"Erro ao carregar dados: {ex.Message}");
        // }
    }

    public void CadastrarJogo()
    {
        Console.Write("Nome do jogo: ");
        string nome = Console.ReadLine() ?? "";
        Console.Write("Categoria: ");
        string categoria = Console.ReadLine() ?? "";
        Console.Write("Idade mínima: ");
        
        if (!int.TryParse(Console.ReadLine(), out int idade))
        {
            throw new ArgumentException("Idade deve ser um número válido");
        }
        
        AdicionarJogo(nome, categoria, idade);
        Console.WriteLine("Jogo cadastrado com sucesso!");
    }

    public void CadastrarMembro()
    {
        Console.Write("Nome: ");
        string nome = Console.ReadLine() ?? "";
        Console.Write("Email: ");
        string email = Console.ReadLine() ?? "";
        Console.Write("Telefone: ");
        string telefone = Console.ReadLine() ?? "";
        Console.Write("Código do membro (número): ");
        
        if (!int.TryParse(Console.ReadLine(), out int codigoMembro))
        {
            throw new ArgumentException("Código deve ser um número válido");
        }
        
        AdicionarMembro(nome, email, telefone, codigoMembro);
        Console.WriteLine("Membro cadastrado com sucesso!");
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
        Console.Write("ID do jogo: ");
        if (!int.TryParse(Console.ReadLine(), out int idJogo))
        {
            throw new ArgumentException("ID do jogo deve ser um número válido");
        }
        
        Console.Write("Código do membro: ");
        if (!int.TryParse(Console.ReadLine(), out int codigoMembro))
        {
            throw new ArgumentException("Código do membro deve ser um número válido");
        }
        
        RealizarEmprestimo(idJogo, codigoMembro);
        Console.WriteLine("Jogo emprestado com sucesso!");
    }

    public void DevolverJogo()
    {
        Console.Write("ID do jogo: ");
        if (!int.TryParse(Console.ReadLine(), out int idJogo))
        {
            throw new ArgumentException("ID do jogo deve ser um número válido");
        }
        
        RealizarDevolucao(idJogo);
        Console.WriteLine("Jogo devolvido com sucesso!");
    }

}

public class DadosBiblioteca
{
    public List<Jogo> Jogos { get; set; } = new List<Jogo>();
    public List<Membro> Membros { get; set; } = new List<Membro>();
    public List<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
    public int ProximoIdJogo { get; set; }
    public int ProximoIdMembro { get; set; }
    public int ProximoIdEmprestimo { get; set; }
}