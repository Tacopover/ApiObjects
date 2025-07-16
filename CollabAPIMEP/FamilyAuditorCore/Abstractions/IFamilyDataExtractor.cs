using FamilyAuditorCore.Models;
using System;

namespace FamilyAuditorCore.Abstractions
{
    /// <summary>
    /// Interface for extracting family information from a CAD system (like Revit)
    /// This will be implemented by the CollabAPIMEP projects to bridge the gap
    /// between Revit API objects and our abstracted FamilyInfo model
    /// </summary>
    public interface IFamilyDataExtractor
    {
        /// <summary>
        /// Extracts family information from CAD system objects into our abstracted model
        /// </summary>
        /// <param name="familyDocument">The family document object from the CAD system</param>
        /// <param name="filePath">Path to the family file</param>
        /// <returns>Abstracted family information</returns>
        FamilyInfo ExtractFamilyInfo(object familyDocument, string filePath);
        
        /// <summary>
        /// Gets the parameter count from the family manager
        /// </summary>
        /// <param name="familyManager">The family manager object from the CAD system</param>
        /// <returns>Number of parameters</returns>
        int GetParameterCount(object familyManager);
        
        /// <summary>
        /// Validates if a family document can be processed by this extractor
        /// </summary>
        /// <param name="familyDocument">The family document to validate</param>
        /// <returns>True if this extractor can process the document</returns>
        bool CanProcess(object familyDocument);
    }
    
    /// <summary>
    /// Exception thrown when family data extraction fails
    /// </summary>
    public class FamilyDataExtractionException : Exception
    {
        public FamilyDataExtractionException(string message) : base(message) { }
        public FamilyDataExtractionException(string message, Exception innerException) : base(message, innerException) { }
    }
}