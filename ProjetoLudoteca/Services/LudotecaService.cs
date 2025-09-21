using Ludoteca.models
using System.Text.Json

namespace Ludoceteca.Service;


public class BibliotecaJogos
{
    private List<Jogo> jogos;

    private List<Membro> membros;

    private List<Emprestimos> emprestimos;

    private int proximoIdJogo;

    private int proximoIdEmprestimo;


    public BibliotecaJogos()
    {
        jogos = new List<Jogo>();
        membros = new List<Membro>();
        //emprestimos = new List<Emprestimo>();

        proximoIdJogo = 1;
        proximoIdEmprestimo = 1;

        //CriarDiretorioSeNaoExistir();
        //Carregar(); 
    }
}