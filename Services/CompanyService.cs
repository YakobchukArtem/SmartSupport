using SmartSupport.Models;
using SmartSupport.Repositories.Interfaces;
using SmartSupport.Services.Dto;
using SmartSupport.Services.Interfaces;

namespace SmartSupport.Services
{
    public class CompanyService(ICompanyRepository companyRepository) : ICompanyService
    {
        public async Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync()
        {
            var companies = await companyRepository.GetAllAsync();
            return companies.Select(company => new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                DocumentationUrl = company.DocumentationUrl,
                Categories = company.Categories,
                OwnerId = company.OwnerId,
                ChatIds = company.Chats.Select(c => c.Id).ToList()
            });
        }

        public async Task<CompanyDto?> GetCompanyByIdAsync(Guid id)
        {
            var company = await companyRepository.GetByIdAsync(id);
            if (company == null)
                return null;

            string? documentationText = null;

            if (!string.IsNullOrWhiteSpace(company.DocumentationUrl) && File.Exists(company.DocumentationUrl))
            {
                documentationText = await File.ReadAllTextAsync(company.DocumentationUrl);
            }

            return new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                DocumentationText = documentationText, // додай це поле в DTO
                Categories = company.Categories,
                OwnerId = company.OwnerId,
                ChatIds = company.Chats.Select(c => c.Id).ToList()
            };
        }

        public async Task CreateCompanyAsync(CompanyCreateDto companyDto)
        {
            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = companyDto.Name,
                DocumentationUrl = @$"D:\Studying\Companies\{companyDto.Name}.txt",
                OwnerId = "9de058b4-304c-4b74-bf69-6b09ab1e7a92"
            };

            await companyRepository.AddAsync(company);
        }

        public async Task UpdateCompanyAsync(Guid id, CompanyUpdateDto companyDto)
        {
            var company = await companyRepository.GetByIdAsync(id);
            if (company == null)
                throw new KeyNotFoundException($"Company with ID {id} not found.");

            if (companyDto.Name != null)
                company.Name = companyDto.Name;

            if (companyDto.DocumentationUrl != null)
                company.DocumentationUrl = companyDto.DocumentationUrl;

            if (companyDto.Categories != null)
                company.Categories = companyDto.Categories;

            await companyRepository.UpdateAsync(company);
        }

        public async Task DeleteCompanyAsync(Guid id)
        {
            var company = await companyRepository.GetByIdAsync(id);
            if (company == null)
                throw new KeyNotFoundException($"Company with ID {id} not found.");

            await companyRepository.DeleteAsync(id);
        }
    }
}
