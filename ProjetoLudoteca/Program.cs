using System;
//using System.Collections;
//using System.Linq.Expressions;
using System.IO;
using Ludoteca.Services; 

class Program
{
    static void Main(string[] args)
    {
        BibliotecaJogos ludoteca = new BibliotecaJogos();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== LUDOTECA .NET ===");
            Console.WriteLine("1 - Cadastrar jogo");
            Console.WriteLine("2 - Cadastrar membro");
            Console.WriteLine("3 -  Listar jogos");
            Console.WriteLine("4 - Emprestar jogo");
            Console.WriteLine("5 - Devolver jogo");
            Console.WriteLine("6 - Gerar relatório");
            Console.WriteLine("0 - Sair");
            Console.Write("Opção: ");

            string opcao = Console.ReadLine() ?? ""; 

            try // [AV1-5]
            {
                switch (opcao)
                {
                    case "1": // [AV1-4-CadastrarJogo]
                        Console.Clear();
                        ludoteca.CadastrarJogo();
                        break;
                    case "2": // [AV1-4-CadastrarMembro]
                        Console.Clear();
                        ludoteca.CadastrarMembro();
                        break;
                    case "3": // [AV1-4-Listar]
                        Console.Clear();
                        ludoteca.ListarJogos();
                        break;
                    case "4": // [AV1-4-Emprestar]
                        Console.Clear();
                        ludoteca.EmprestarJogo();
                        break;
                    case "5": // [AV1-4-Devolver]
                        Console.Clear();
                        ludoteca.DevolverJogo();
                        break;
                    case "6": // [AV1-4-Relatorio]
                        Console.Clear();
                        ludoteca.GerarRelatorio();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opção inválida!");
                        break;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("Data/debug.log", $"[{DateTime.Now}] Erro: {ex.Message}\n");
                Console.WriteLine("Erro ao executar a operação.");
            }

            Console.WriteLine("\nPressione Enter para continuar.");
            Console.ReadLine();
        }
    }
}

