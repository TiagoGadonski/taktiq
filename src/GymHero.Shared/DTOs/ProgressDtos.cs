namespace GymHero.Application.DTOs;

// DTO para enviar os dados de uma nova métrica
public record LogProgressMetricRequest(
    string Type, // Ex: "Peso", "Cintura", "Braço"
    double Value,
    string Unit, // Ex: "kg", "cm"
    DateTime Date
);

