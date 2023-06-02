describe("landing page", () => {
  const url = Cypress.env("URL");

  const expectedFeedbackLink = "https://feedback";

  beforeEach(() => {
    cy.visit(url);
  });

  it("should contain title", () => {
    cy.get("h1.govuk-heading-xl").should("exist");
  });

  it("should contain headings", () => {
    cy.get("h2.govuk-heading-m")
      .should("exist")
      .and("have.length.of.at.least", 4);
  });

  it("should contain text bodies", () => {
    cy.get("p.govuk-body").should("exist").and("have.length.of.at.least", 4);
  });

  it("should have bold texts", () => {
    cy.get("span.govuk-\\!-font-weight-bold").should("exist");
  });

  it("should have unordered list", () => {
    cy.get("ul").should("exist");
  });

  it("should have list items", () => {
    cy.get("ul li").should("exist").and("have.length.of.at.least", 4);
  });
});