using StudyBuddy.Application.Interfaces;
using StudyBuddy.Application.Models;

namespace StudyBuddy.Application.Eval;

/// <summary>
/// Hardcoded per-mode eval fixtures grounded in SOLID study material
/// already used for functional verification of the tutoring modes.
/// </summary>
public sealed class HardcodedEvalTestSetProvider : IEvalTestSetProvider
{
    public const string SolidPrinciplesMaterial =
        """
        SOLID is a set of five design principles that make object-oriented software easier to maintain and extend.

        Single Responsibility Principle (SRP): A class should have only one reason to change. Each class owns a single concern so changes stay local.

        Open/Closed Principle (OCP): Software entities should be open for extension but closed for modification. Prefer adding new behaviour through new types rather than editing existing ones.

        Liskov Substitution Principle (LSP): Subtypes must be substitutable for their base types without breaking callers. A derived class must honour the contracts of its parent.

        Interface Segregation Principle (ISP): Clients should not be forced to depend on methods they do not use. Prefer small, focused interfaces over large ones.

        Dependency Inversion Principle (DIP): High-level modules should not depend on low-level modules; both should depend on abstractions. Depend on interfaces, not concrete implementations.

        Dependency injection is a practical technique for applying DIP: instead of a class constructing its own dependencies, those dependencies are provided from the outside (constructor injection is the most common form).
        """;

    public const string DependencyInjectionMaterial =
        """
        Dependency injection means a class receives the services it needs from the outside rather than creating them itself.
        Constructor injection is the usual approach in .NET: declare required dependencies as constructor parameters and let the DI container supply implementations.
        This makes code easier to test (swap in fakes) and keeps high-level policy independent of low-level details, which is the Dependency Inversion Principle in practice.
        """;

    public IReadOnlyList<EvalTestCase> GetExplainCases() =>
    [
        new EvalTestCase("Explain-SRP", SolidPrinciplesMaterial, "What is the Single Responsibility Principle?"),
        new EvalTestCase("Explain-OCP", SolidPrinciplesMaterial, "Explain the Open/Closed Principle in plain language."),
        new EvalTestCase("Explain-DI", DependencyInjectionMaterial, "What is dependency injection?"),
        new EvalTestCase("Explain-DIP-DI", SolidPrinciplesMaterial, "How does Dependency Inversion relate to dependency injection?")
    ];

    public IReadOnlyList<EvalTestCase> GetQuizCases() =>
    [
        new EvalTestCase("Quiz-SRP", SolidPrinciplesMaterial, "Single Responsibility Principle"),
        new EvalTestCase("Quiz-LSP", SolidPrinciplesMaterial, "Liskov Substitution Principle"),
        new EvalTestCase("Quiz-DI", DependencyInjectionMaterial, "Dependency injection"),
        new EvalTestCase("Quiz-ISP", SolidPrinciplesMaterial, "Interface Segregation")
    ];

    public IReadOnlyList<EvalTestCase> GetSummariseCases() =>
    [
        new EvalTestCase("Summarise-SOLID", SolidPrinciplesMaterial),
        new EvalTestCase("Summarise-DI", DependencyInjectionMaterial),
        new EvalTestCase(
            "Summarise-SOLID-why",
            SolidPrinciplesMaterial + "\n\nWhy it matters: following SOLID reduces coupling, makes unit testing practical, and keeps features cheaper to change over time.")
    ];
}
