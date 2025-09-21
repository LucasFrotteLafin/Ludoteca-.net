using Ludoteca.models
using System.Text.Json

namespace Ludoteca.Service;


public class BibliotecaJogos
{
    private List<Jogo> jogos;

    private List<Membro> membros;

    private List<Emprestimo> emprestimos;

    private int proximoIdJogo;

    private int proximoIdEmprestimo;


    public BibliotecaJogos()
    {
        jogos = new List<Jogo>();
        membros = new List<Membro>();
        emprestimos = new List<Emprestimo>();

        proximoIdJogo = 1;
        proximoIdEmprestimo = 1;
        //Carregar(); 
    }

    public void CadastrarJogo(string nome, string categoria, int idadeMinima)
    {
        // Verificar se o jogo já existe
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
            throw new Exception("Jogo já existe!")
        }

        Jogo novoJogo = new Jogo(proximoIdJogo, nome, categoria, idadeMinima);
        jogos.Add(novoJogo);
        proximoIdJogo = proximoIdJogo++;
        //SalvarDados()
    }

    public void CadastrarMembro(string nome,string email,string telefone,int senha)
    {
        // Verificar se o email já existe
        bool emailExiste = false;
        for (int i = 0; i < membros.Count; int++)
        {
            if (membros[i].Email.ToLower() == email.ToLower())
            {
                emailExiste = true;
                break;
            }
        }

        if (emailExiste)
        {
            throw new Exception("Email já existe!");
        }

        // Verificar se a senha já existe
        bool senhaExiste = false;
        for (int i = 0; i < membros.Count; i++)
        {
            if (membros[i].Senha == senha)
            {
                senhaExiste = true;
                break;
            }

            if (senhaExiste)
            {
                throw new Exception("Senha existente, tente novamente")
            }

            //Adicionando novo membro
            Membro novoMembro = new Membro(senha,nome,email,telefone);
            membros.Add(novoMembro);
            //SalvarDados();
        }
    }

    public bool AutenticarMembro(int senha)
    {
        for (int i = 0; i < membros.Count; i++)
        {
            if (membros[i].Senha == senha)
            {
                return true;
            }
        }
        return false;
    }

    public Membro BuscarPorMembro(int senha)
    {

        for (int i = 0; i < membros.Count; i++)
        {
            if (membros[i].Senha == senha)
            {
                return membros[i];
            }
        }
        return null;
    }

    public List<Jogo> ListarJogos()
    {
        // Listar os jogos
        List<Jogo> copia = new List<Jogo>();
        for (int i = 0; i < jogos.Count; i++)
        {
            copia.Add(jogos[i]);
        }
        return copia;
    }

    public void EmprestarJogo(int idJogo, int senhaMembro)
    {
        // Buscar jogo
        Jogo jogo = null;
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
            throw new Exception("Jogo não encontrado!");
        }

        // Buscar membro pela senha 
        Membro membro = null;
        for (int i = 0; i < membro.Count; i++)
        {
            if (membro[i].Senha == senhaMembro)
            {
                membro = membros[i];
                break;
            }
        }

        if (membro == null)
        {
            throw new Exception("Senha inválida!");
        }

        //if (jogo.EstaEmprestado)
        //{
        //    throw new Exception("Jogo já está emprestado!");
        //}

        //jogo.MarcarComoEmprestado();
        //emprestimos novoEmprestimo = new Emprestimo(proximoIdEmprestimo, idJogo, 7);
        //emprestimos.Add(novoEmprestimo);
        //proximoIdEmprestimo = proximoIdEmprestimo++; 
        ////SalvarDados()
    }
    
    public void DevolverJogo(int idJogo)
    {
        // Buscar jogo
        Jogo jogo = null;
        for (int i = 0; i< jogos.Count; i++)
        {
            if (jogos[i].Id == idJogo)
            {
                jogo = jogos[i];
                break;
            }
        }

        if (jogo == null)
        {
            throw new ArgumentException("Jogo nao encontrado!", nameof(idJogo));
        }
    }
}