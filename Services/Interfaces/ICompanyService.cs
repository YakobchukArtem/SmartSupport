using SmartSupport.Services.Dto;

namespace SmartSupport.Services.Interfaces;
public interface ICompanyService
{
    Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync();
    Task<CompanyDto?> GetCompanyByIdAsync(Guid id);
    Task CreateCompanyAsync(CompanyCreateDto companyDto);
    Task UpdateCompanyAsync(Guid id, CompanyUpdateDto companyDto);
    Task DeleteCompanyAsync(Guid id);
}
