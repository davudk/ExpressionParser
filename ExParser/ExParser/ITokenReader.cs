namespace ExParser {
    interface ITokenReader {
        Token GetToken();
        Token PeekToken();
    }
}
