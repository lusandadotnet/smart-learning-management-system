namespace SmartLMS.Domain.Entities;

public class AiTutorConfiguration
{
    public int Id { get; set; }
    
    // Foreign Key
    public int CourseId { get; set; }

    // The core instructions 
    public required string SystemPrompt { get; set; }

    // The Azure OpenAI Deployment Name 
    public required string DeploymentName { get; set; }

    // Controls creativity 
    public float Temperature { get; set; } = 0.7f;

    // The maximum length of the AI's response
    public int MaxTokens { get; set; } = 800;

    // nav prop
    public virtual Course Course { get; set; } = null!;
}