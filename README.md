<h1 align="center">
  <a href="https://github.com/dec0dOS/amazing-github-template">
    <img src="{{cookiecutter.repo_slug}}/docs/images/logo.svg" alt="Logo" width="125" height="125">
  </a>
</h1>

<div align="center">
  Amazing GitHub Template - Sane defaults for your next project!
  <br />
  <br />
  <a href="https://github.com/dec0dOS/amazing-github-template/issues/new?assignees=&labels=bug&template=01_BUG_REPORT.md&title=bug%3A+">Report a Bug</a>
  ·
  <a href="https://github.com/dec0dOS/amazing-github-template/issues/new?assignees=&labels=enhancement&template=02_FEATURE_REQUEST.md&title=feat%3A+">Request a Feature</a>
  .
  <a href="https://github.com/dec0dOS/amazing-github-template/discussions">Ask a Question</a>
</div>

<div align="center">
<br />

[![license](https://img.shields.io/github/license/dec0dOS/amazing-github-template.svg?style=flat-square)](LICENSE)

[![PRs welcome](https://img.shields.io/badge/PRs-welcome-ff69b4.svg?style=flat-square)](https://github.com/dec0dOS/amazing-github-template/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22)
[![made with hearth by dec0dOS](https://img.shields.io/badge/made%20with%20%E2%99%A5%20by-dec0dOS-ff1414.svg?style=flat-square)](https://github.com/dec0dOS)

</div>

<details open="open">
<summary>Table of Contents</summary>

- [About](#about)
  - [Built With](#built-with)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Usage](#usage)
    - [Cookiecutter template](#cookiecutter-template)
    - [Manual setup](#manual-setup)
    - [Variables reference](#variables-reference)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [Support](#support)
- [License](#license)
- [Acknowledgements](#acknowledgements)

</details>

---

## About

<table>
<tr>
<td>

Open Source Software is not about the code in the first place but the communications and community. People love good documentation and obvious workflows. If your software solves some problem, but nobody can figure out how to use it or, for example, how to create an effective bug report, there's something very bad going on. Did you hear about Readme Driven Development? Check out the awesome [article written by GitHub co-founder Tom Preston-Werner](https://tom.preston-werner.com/2010/08/23/readme-driven-development.html).

There are many great README or issues templates available on GitHub, however, you have to find them yourself and combine different templates yourself. In addition, if you want extensive docs like CODE_OF_CONDUCT.md, CONTRIBUTING.md, SECURITY.md or even advanced GitHub features like a pull request template, additional labels, code scanning, and automatic issue/PR closing and locking you have to do much more work. Your time should be focused on creating something **amazing**. You shouldn't be doing the same tasks over and over like creating your GitHub project template from scratch. Follow the **don’t repeat yourself** principle. Use a template **and go create something amazing**!

Key features of **Amazing GitHub Template**:

- Configurable README.md template
- Configurable LICENSE template
- Configurable CODE_OF_CONDUCT.md template
- Configurable CONTRIBUTING.md template
- Configurable SECURITY.md template
- Configurable issues template
- Pull request template
- CODEOWNERS template
- Additional labels template
- Automatic locking for closed issues and PRs workflow
- Automatic cleaning for stale issues and PRs workflow
- Automatic label verification for PRs workflow
- Automatic security code scanning workflow via CodeQL

<details open>
<summary>Additional info</summary>
<br>

This project is the result of huge research. I'm a long-time GitHub user so I've seen more than [7.3k](https://github.com/dec0dOS?tab=stars) READMEs so far. I've started writing docs for my open source projects (that are currently in their early stages so they exist in the private space for now). After I've analyzed many popular GitHub READMEs and other GitHub-related docs and features I've tried to create a general-propose template that may be useful for any project.

Of course, no template will serve all the projects since your needs may be different. So [Cookiecutter](https://github.com/cookiecutter/cookiecutter) comes to the rescue. It allows [Jinja template language](https://jinja.palletsprojects.com) to be used for complex cases. Just enter up the project preferences you want in the Cookiecutter interactive menu and that's it. There is a manual setup that could be useful for your existing projects (or if you don't want to use Cookiecutter for some reason). **This README.md file is not a template itself**, you should [download the precompiled template](https://github.com/dec0dOS/amazing-github-template/releases/download/latest/template.zip) and replace the predefined values, then remove unused sections.

</details>

</td>
</tr>
</table>

### Built With

- [GitHub Flavored Markdown Spec](https://github.github.com/gfm/)
- [Cookiecutter](https://github.com/cookiecutter/cookiecutter)
- [GitHub Actions](https://github.com/features/actions)
- [markdownlint-cli](https://github.com/igorshubovych/markdownlint-cli)

## Getting Started

### Prerequisites

The recommended method to install **Amazing GitHub Template** is by using [Cookiecutter](https://github.com/cookiecutter/cookiecutter). For manual install please refer to [manual setup section](#manual-setup).

```sh
pip install --user cookiecutter
```

### Usage

#### Manual setup

Manual seup

## Contributing

First off, thanks for taking the time to contribute! Contributions are what makes the open-source community such an amazing place to learn, inspire, and create. Any contributions you make will benefit everybody else and are **greatly appreciated**.

Please try to create bug reports that are:

- _Reproducible._ Include steps to reproduce the problem.
- _Specific._ Include as much detail as possible: which version, what environment, etc.
- _Unique._ Do not duplicate existing opened issues.
- _Scoped to a Single Bug._ One bug per report.

## Support

Reach out to the maintainer at one of the following places:

- [GitHub discussions](https://github.com/dec0dOS/amazing-github-template/discussions)
- The email which is located [in GitHub profile](https://github.com/dec0dOS)

## License

This project is licensed under the **MIT license**. Feel free to edit and distribute as you like.

See [LICENSE](LICENSE) for more information.

## Acknowledgements
