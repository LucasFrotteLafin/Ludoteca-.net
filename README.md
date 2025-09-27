# ProjetoLudoteca - DESING E POO

Sistema de gerenciamento de ludoteca desenvolvido em C# .NET 9.0 para disciplina de Design e Programação Orientada a Objetos.

## Funcionalidades

### Menu Principal
1. Cadastrar jogo
2. Cadastrar membro  
3. Listar jogos
4. Emprestar jogo
5. Devolver jogo
6. Gerar relatório
7. Verificar multa
8. Recarregar dados
0. Sair

### Gestão de Jogos
- Cadastro com ID automático, nome, categoria e idade mínima
- Controle de disponibilidade (disponível/emprestado)
- Validação de idade mínima (0-18 anos)

### Gestão de Membros
- Cadastro com código único, nome, email, telefone e data de nascimento
- Cálculo automático da idade
- Validação de email (formato padrão) e telefone (formato brasileiro)
- Verificação de idade para empréstimos

### Sistema de Empréstimos
- Empréstimos automáticos de 7 dias
- Verificação de idade (membro deve ter idade mínima do jogo)
- Controle de status ativo/devolvido
- Cálculo de multas por atraso (R$ 2,50 por dia)

### Persistência e Relatórios
- Dados salvos em JSON (Data/biblioteca.json)
- Log de erros (Data/debug.log)
- Relatórios completos (Data/relatorio.txt)
- Formato de datas brasileiro

## Estrutura do Projeto

```
ProjetoLudoteca/
├── Models/
│   ├── Jogo.cs          # Classe de jogos
│   ├── Membro.cs        # Classe de membros
│   └── Emprestimo.cs    # Classe de empréstimos
├── Services/
│   ├── LudotecaService.cs    # Lógica principal (BibliotecaJogos)
│   └── DateTimeConverter.cs  # Conversor de datas JSON
├── Data/
│   ├── biblioteca.json  # Dados persistidos
│   ├── debug.log       # Log de erros
│   └── relatorio.txt   # Relatórios gerados
└── Program.cs          # Interface console
```

## Conceitos de POO Aplicados

- **Encapsulamento**: Propriedades com `private set`
- **Validação**: Métodos privados de validação
- **Constantes**: Valores fixos como `CODIGO_MAXIMO`, `IDADE_MAXIMA_PERMITIDA`
- **Tratamento de Exceções**: Try-catch com logs
- **Serialização**: JSON com System.Text.Json
- **Separação de Responsabilidades**: Models, Services, Program

## UML
[https://drive.google.com/file/d/12O7z3cC2zo8BqCUs5LKNd9w1FNXRLkOX/view?usp=sharing](https://drive.google.com/file/d/12O7z3cC2zo8BqCUs5LKNd9w1FNXRLkOX/view?usp=sharing)

## Vídeo Explicativo
https://drive.google.com/file/d/1t0OZelsp0HYn7SnbkQW_7PtMOGfa6FlT/view?usp=sharing

## Tecnologias
- C# .NET 9.0
- System.Text.Json
- Regex para validações
- Console Application

## Autores
- 06009322 - Ana Luiza Maciel Mattos
- 06010493 - Lucas Frotté Lafin
- 06010196 - Pedro Nogueira Teodosio 
- 06010479 - Alexandre dos Santos Firmino
- 06010096 - Ana Carolina da Costa Tomás da Rocha 
