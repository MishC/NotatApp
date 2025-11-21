namespace NotatApp.Models{
//DTO- data transfer object
//What will come from frontend
public record RegisterDto(string Email, string Password, string? PhoneNumber);
public record LoginDto(string Email, string Password, string Channel);
public record Verify2FaDto(string FlowId, string Code, string Channel);


};