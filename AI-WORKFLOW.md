# AI Workflow

This document captures how AI assistance was used in this repository and how outputs were reviewed.

## Tools Used

- I used ChatGPT for initial planning of the architecture and to validate my understanding of the requirements and constraints, and the minimal folder structure needed to implement the features.
- For 90-95% tasks GitHub Copilot (GPT-5.2-Codex) for code generation, code review support, and troubleshooting guidance.
- Local codebase inspection to verify architecture rules and required API surface.
- In some cases Claude Sonnet 4.6 was used to solve problems that GPT-5.2-Codex struggled with, such as detecting the issue with Swagger and OpenAPI compatibility in the API project.

## Planning

- I conversed with ChatGPT to validate my initial understanding of the architecture of the project and the infrastructure components like DB, message broker, and workers.
- Then I used the files in .context/ to prime the Copilot models to generate project structure.
- After every step of generation, I reviewed the generated content to ensure it aligned with the principles and architecture rules defined in the .context/ files and to my knowledge of .NET, Node.js and system design best practices.

## Code Generation

- I gave the AI model definitions, architecture rules, and examples of existing code to generate new code for the API, workers, and documentation.
- I intervened where the AI drifted from the rules, and corrected the generated code iteratively until it met the requirements.
- I had the AI write tests, and verify code correctness by verifying build and test pass.

## Debugging and Iteration

- When AI generated code that did not meet the constraints, or did not compile or pass tests, I pointed out key issues and had it regenerate the code.
- Documentation was iterated to align with Docker and manual setup paths defined in [docker-compose.yml](docker-compose.yml).

## Model Behaviour

- GPT-5.2-Codex produced consistent code, but sometimes missed specific constraints around scale and failure modes, which required me to explicitly prompt for those details.
- It required explicit guidance on what to prioritize (limitations and scale constraints) to avoid overly generic content.
- GPT-5.2 Codex has a larger context window than Sonnet 4.6, which made it more effective for generating code that needed to adhere to specific architecture rules and patterns defined in the .context/ files.
- AI generated code failed to meet requirements on multiple occasions. One time it generated code with a hardcoded RabbitMQ username and password, which I had to point out and have it fix by removing defaults and validating config values. Another time it generated code that did not compile due to OpenAPI version compatibility issues, which I had to solve by switching to Sonnet 4.6 for better troubleshooting guidance. Another example is, for audit worker operations, it initially generated code without idempotency, retries, DLQ, or health endpoint, which I had to explicitly instruct it to add.

## Reflection

- AI assistance definitely works very well for generating boilerplate code, documentation and even for straightforward implementation of features that are well within the model's training data and capabilities.
- However, for key architectural level constraints, human intervention is still required to ensure that the generated content aligns with the intended design and operational requirements, especially around scale and failure modes.
- Future runs should include a short checklist of scale and failure-mode constraints to avoid omissions.
- For complex business logic, I would advise thorough review of generated code and tests, as the AI may not always capture edge cases or specific requirements without explicit prompting.
- Copilot's agentic behavior such as terminal access and file editing saved hours of manual work, but it also requires careful inspection, manual run of tests and builds, and iterative prompting to ensure the generated code is correct and meets the requirements.
