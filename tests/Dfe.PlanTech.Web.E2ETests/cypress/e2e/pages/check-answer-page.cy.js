let selectedQuestionsWithAnswers = [];

describe("Check answers page", () => {
  const url = "/self-assessment";

  beforeEach(() => {
    selectedQuestionsWithAnswers = [];
    cy.loginWithEnv(url);

    navigateToCheckAnswersPage();

    cy.log(selectedQuestionsWithAnswers);

    cy.url().should("contain", "check-answers");

    cy.injectAxe();
  });

  it("should show each selected question with answer", () => {
    cy.get("div.govuk-summary-list__row")
      .should("exist")
      .and("have.length", selectedQuestionsWithAnswers.length)
      .each((row) => {
        //Get question and answer tecxt for each row
        const questionWithAnswer = {
          question: null,
          answer: null,
        };

        cy.wrap(row)
          .find("dt.govuk-summary-list__key.spacer")
          .should("exist")
          .invoke("text")
          .then((question) => (questionWithAnswer.question = question.trim()));

        cy.wrap(row)
          .find("dd.govuk-summary-list__value.spacer")
          .invoke("text")
          .then((answer) => (questionWithAnswer.answer = answer.trim()));

        //Ensure it matches one of the items in array
        cy.wrap(questionWithAnswer).then(() => {
          cy.log(JSON.stringify(selectedQuestionsWithAnswers));

          const matchingQuestionWithAnswer = selectedQuestionsWithAnswers.find(
            (qwa) => {
              cy.log("looking for ", qwa);
              return qwa.question == questionWithAnswer.question.trim();
            }
          );

          expect(matchingQuestionWithAnswer.answer).to.equal(
            questionWithAnswer.answer
          );

          //Has "Change" me link with accessibility attributes
          cy.wrap(row)
            .find("a")
            .contains("Change")
            .and("have.attr", "aria-label")
            .and("contain", questionWithAnswer.question);

          cy.wrap(row)
            .find("a")
            .contains("Change")
            .and("have.attr", "title")
            .and("equal", questionWithAnswer.question);
        });
      });
  });

  it("passes accessibility tests", () => {
    cy.runAxe();
  });

  it("navigates to correct page when clicking change", () => {
    cy.get("a:nth-child(1)").contains("Change").click();
    cy.url().should("contains", "broadband-contract-review");
  });
  
  //This needs to be last on this test run, so that the question-page tests have a clean slate to work from!
  it("submits answers and shows notification", () => {
    submitAnswers();

    cy.url().should("contain", "self-assessment");
    cy.get("div.govuk-notification-banner__header").should("exist");
  });

});

const navigateToCheckAnswersPage = () => {
  cy.clickFirstSection();
  cy.clickContinueButton();

  return navigateThroughQuestions();
};

const navigateThroughQuestions = () => {
  cy.get("main")
    .then(($main) => $main.find("form div.govuk-radios").length > 0)
    .then((onQuestionPage) => {
      if (!onQuestionPage) {
        return Promise.resolve();
      }

      cy.selectFirstRadioButton().then((questionWithAnswer) =>
        selectedQuestionsWithAnswers.push(questionWithAnswer)
      );
      cy.saveAndContinue();

      return navigateThroughQuestions();
    })
    .then(() => {});
};

const submitAnswers = () =>
  cy.get("button.govuk-button").contains("Save and Submit").click();