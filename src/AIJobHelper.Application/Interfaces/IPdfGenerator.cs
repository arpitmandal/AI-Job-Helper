namespace AIJobHelper.Application.Interfaces;

public interface IPdfGenerator
{
    /// <summary>Generates a PDF from the three cover letter sections and returns the bytes.</summary>
    byte[] Generate(string header, string body, string footer, string fileName);
}
