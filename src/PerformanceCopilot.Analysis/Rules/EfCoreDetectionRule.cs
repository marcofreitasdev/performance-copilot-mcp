using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerformanceCopilot.Analysis.Models;

namespace PerformanceCopilot.Analysis.Rules;

public sealed class EfCoreDetectionRule : IDetectionRule
{
    public string PatternType => "efcore";

    private static readonly HashSet<string> EfCoreIdentifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "DbSet", "DbContext", "DbContextOptions"
    };

    private static readonly HashSet<string> EfCoreMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Include", "ThenInclude", "ToListAsync", "ToArrayAsync", "FirstOrDefaultAsync",
        "SingleOrDefaultAsync", "FirstAsync", "SingleAsync", "CountAsync", "AnyAsync",
        "SumAsync", "MaxAsync", "MinAsync", "AverageAsync", "SaveChangesAsync",
        "SaveChanges", "AddAsync", "AddRangeAsync", "UpdateRange", "RemoveRange",
        "Entry", "FromSqlRaw", "FromSqlInterpolated", "ExecuteSqlRawAsync",
        "ExecuteSqlInterpolatedAsync"
    };

    public DetectedPattern? Detect(MethodDeclarationSyntax method, SemanticModel _)
    {
        var identifierCount = method.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Count(id => EfCoreIdentifiers.Contains(id.Identifier.Text));

        var methodCount = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(inv => inv.Expression)
            .OfType<MemberAccessExpressionSyntax>()
            .Count(ma => EfCoreMethods.Contains(ma.Name.Identifier.Text));

        var total = identifierCount + methodCount;
        return total > 0 ? new DetectedPattern(PatternType, total) : null;
    }
}
