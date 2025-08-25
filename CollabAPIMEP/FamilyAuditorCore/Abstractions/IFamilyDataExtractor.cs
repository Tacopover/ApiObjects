using FamilyAuditorCore.Models;
using System;

namespace FamilyAuditorCore.Abstractions
{
    /// <summary>
    /// Interface for extracting family information from a CAD system (like Revit)
    /// This abstraction allows the core validation logic to work with different CAD systems
    /// without direct dependencies on their APIs
    /// </summary>
    public interface IFamilyDataExtractor
    {
        /// <summary>
        /// Extracts family information from CAD system objects into our abstracted model
        /// This method bridges the gap between CAD-specific objects and our framework-agnostic data model
        /// </summary>
        /// <param name="familyDocument">The family document object from the CAD system (e.g., Revit Document)</param>
        /// <param name="filePath">Path to the family file on disk</param>
        /// <returns>Abstracted family information that can be validated against rules</returns>
        /// <exception cref="FamilyDataExtractionException">Thrown when extraction fails or document cannot be processed</exception>
        FamilyInfo ExtractFamilyInfo(object familyDocument, string filePath);
        
        /// <summary>
        /// Gets the parameter count from the family manager
        /// This is a specialized extraction method for parameter-specific validation
        /// </summary>
        /// <param name="familyManager">The family manager object from the CAD system (e.g., Revit FamilyManager)</param>
        /// <returns>Number of parameters in the family</returns>
        /// <exception cref="FamilyDataExtractionException">Thrown when parameter count cannot be extracted</exception>
        int GetParameterCount(object familyManager);
        
        /// <summary>
        /// Validates if a family document can be processed by this extractor
        /// This allows for multiple extractors to coexist and handle different CAD system types
        /// </summary>
        /// <param name="familyDocument">The family document to validate</param>
        /// <returns>True if this extractor can process the document, false otherwise</returns>
        bool CanProcess(object familyDocument);
    }
    
    /// <summary>
    /// Exception thrown when family data extraction fails
    /// This provides a standardized way to handle extraction errors across different CAD systems
    /// </summary>
    [Serializable]
    public class FamilyDataExtractionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the FamilyDataExtractionException class
        /// </summary>
        public FamilyDataExtractionException() { }

        /// <summary>
        /// Initializes a new instance of the FamilyDataExtractionException class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public FamilyDataExtractionException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the FamilyDataExtractionException class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public FamilyDataExtractionException(string message, Exception innerException) : base(message, innerException) { }
    }
}