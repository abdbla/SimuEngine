namespace EventParser

open FParsec.CharParsers
open FParsec.Primitives

type SelectorExpr =
  | ThisNode
  | Neighbors

type FunctionalExpr =
  | Set

type PredicateExpr =
  | ForAll  of SelectorExpr * PredicateExpr list
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

    let infix<'left, 'right, 'u> left (cs : char list) right : Parser<'left * char * 'right, 'u> =
        let op = cs |> List.map pchar |> choice |> trimmed
        let mapOpcode a b c : 'left * char * 'right = (a, b, c)
        pipe3 left op right mapOpcode

    let infixPredicate<'u> : Parser<PredicateExpr, 'u> =
        infix propertyName
              ['>'; '<'; '=']
              pint32 |>> fun (s, c, n) -> match c with
                                           | '>' -> Greater (s, n)
                                           | '<' -> Lesser (s, n)
                                           | '=' -> Equal (s, n)
                                           | _   -> failwith "this shouldn't happen"

    let predicateList<'u> : Parser<PredicateExpr list, 'u> =
        sepBy1 infixPredicate
               (pchar ',' |> trimmed) |>
        parenthesized

    let all<'u> : Parser<PredicateExpr, 'u> =
        pstring "all" >>. predicateList |>> All
    
    let any<'u> : Parser<PredicateExpr, 'u> =
        pstring "any" >>. predicateList |>> Any