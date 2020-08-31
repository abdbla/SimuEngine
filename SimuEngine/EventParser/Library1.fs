namespace EventParser

open FParsec.CharParsers
open FParsec.Primitives

type SelectorExpr =
  | ThisNode
  | Neighbors
  | Union of SelectorExpr list

type FunctionalExpr =
  | Set

type PredicateExpr =
  | ForAll  of SelectorExpr * PredicateExpr
  | ForAny  of SelectorExpr * PredicateExpr
  | All     of PredicateExpr list
  | Any     of PredicateExpr list
  | Greater of string * int
  | Lesser  of string * int
  | Equal   of string * int

type Expr =
  | Functional of FunctionalExpr
  | Predicate of PredicateExpr

module Parsers =
    let trimmed<'a, 'u> : Parser<'a, 'u> -> Parser<'a, 'u> =
        between spaces spaces

    let delimited c = between (pchar c) (pchar c)

    let pairMap f (a, b) = (f a, f b)

    let parenthesized<'a, 'u> : Parser<'a, 'u> -> Parser<'a, 'u> = ('(', ')') |> pairMap pchar ||> between

    let propertyName<'u> : Parser<string, 'u> =
        many1Chars2 asciiLetter (asciiLetter <|> digit) <|> delimited '"' (noneOf ['"'] |> manyChars)

    let infix<'left, 'right, 'u> left (cs : string list) right : Parser<'left * string * 'right, 'u> =
        let op = cs |> List.map pstring |> choice |> trimmed
        pipe3 left op right (fun a b c -> (a, b, c))

    let infixChar<'left, 'right, 'u> left (cs : char list) right : Parser<'left * char * 'right, 'u> =
        let op = cs |> List.map pchar |> choice |> trimmed
        pipe3 left op right (fun a b c -> (a, b, c))

    let infixPredicate<'u> : Parser<PredicateExpr, 'u> =
        infixChar propertyName
                  ['>'; '<'; '=']
                  pint32 |>> fun (s, c, n) -> match c with
                                              | '>' -> Greater (s, n)
                                              | '<' -> Lesser (s, n)
                                              | '=' -> Equal (s, n)
                                              | _   -> failwith "this shouldn't happen"

    let predicateList<'u> (p : Parser<PredicateExpr, 'u>) : Parser<PredicateExpr list, 'u> =
        sepBy1 p (pchar ',' |> trimmed) |> parenthesized

    let all<'u> (p : Parser<PredicateExpr, 'u>) : Parser<PredicateExpr, 'u> =
        pstring "all" >>. predicateList p |>> All
    let any<'u>  (p : Parser<PredicateExpr, 'u>) : Parser<PredicateExpr, 'u> =
        pstring "any" >>. predicateList p |>> Any

    let selector<'u> : Parser<SelectorExpr, 'u> =
        let selector' = (pstring "this" >>. preturn ThisNode)
                        <|> (pstring "neighbors" >>. preturn Neighbors)
        sepBy1 selector' (pchar '+' |> trimmed) |>>
            fun sl -> match sl with
                      | [s] -> s
                      | _   -> Union sl

    let forX p (f : (SelectorExpr * PredicateExpr) -> PredicateExpr) =
        (trimmed selector) .>> pchar ';' .>>. (trimmed p) |>> f
        |> parenthesized
        |> trimmed

    let forAll p = pstring "forall" >>. forX p ForAll
                 
    let forAny p = pstring "forany" >>. forX p ForAny

    let rec parse x =
        choice 
            (List.map (fun f -> f parse) [forAny; forAll; all; any] @ [infixPredicate]) x
        
open EngineCore

module Execution =
    let rec resolveSelector (s : SelectorExpr) (node : Node) (graph : Graph) : Node list =
        match s with
        | Union sl -> List.map (fun s -> resolveSelector s node graph) sl |> List.reduce (@)
        | ThisNode -> [node]
        | Neighbors -> graph.GetConnections node |> Seq.toList |> List.map (fun struct(_, b) -> b)

    let rec resolveFor s p f n g1 g2 : bool =
        resolveSelector s n g1
        |> List.map (fun node -> convertPredicate p node g1 g2)
        |> List.reduce f
    and convertPredicate (p : PredicateExpr) : (Node -> Graph -> Graph -> bool) =
        fun (n : EngineCore.Node) (g1 : EngineCore.Graph) (g2 : EngineCore.Graph) ->
            let traits : System.Collections.Generic.Dictionary<string, int> = n.traits;
            let predicate f s i =
                let found, value = traits.TryGetValue s
                if found then f value i else false
            let mapping ps f =
                ps |> List.map (convertPredicate >> (fun f -> f n g1 g2)) |> List.reduce f
            match p with
            | ForAll (s, p)     -> resolveFor s p (&&) n g1 g2
            | ForAny (s, p)     -> resolveFor s p (||) n g1 g2
            | All ps            -> mapping ps (&&)
            | Any ps            -> mapping ps (||)
            | Greater (s, i)    -> predicate (>) s i
            | Lesser (s, i)     -> predicate (<) s i
            | Equal (s, i)      -> predicate (=) s i


type Parser(inner : Expr) =
    new(str : string) =
        Parser(
            Predicate (match run Parsers.parse str with
                       | Success(result, _, _) -> result
                       | Failure(errorMsg, _, _) -> failwith ("Failure: " + errorMsg)))

    member this.toFunction() =
        match inner with
        | Functional _ -> failwith "lol"
        | Predicate p -> System.Func<Node, Graph, Graph, bool> (Execution.convertPredicate p)