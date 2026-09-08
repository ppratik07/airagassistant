namespace EnterpriseChat.Application.Models;

//record - immutable, hashable, comparable, serializable, deconstructable, etc.
//sealed - cannot be inherited

public sealed record ChatRequest(string Message);