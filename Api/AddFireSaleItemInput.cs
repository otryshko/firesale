public record AddFireSaleItemInput(
    string Name,
    Decimal Price,
    int Quantity,
    DateTime BeginAtUtc);