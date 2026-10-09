# Web Frontend Høst 2026

## Om kurset

Denne utdanningen gir deg en dyptgående innføring i moderne frontend-utvikling, fra grunnleggende HTML og CSS til avanserte single page applications. Du vil lære responsive design-prinsipper, objektorientert og funksjonell programmering med TypeScript, og hvordan bygge skalérbare applikasjoner med moderne rammeverk.

Kurset forbereder deg grundig på virkelige utfordringer i arbeidslivet og gir deg kompetansen som trengs for å lykkes som profesjonell frontend-utvikler.

### Studiets innhold

- Varighet: 20 uker
- Studiepoeng: 30
- Undervisningsform: 100 % nettbasert
- Forkunnskaper: Bygger på utdanningen Introduksjon til programmering og IT-utvikling
- Opptak via realkompetanse: Hvis du fyller minst 23 år i oppstartsåret og har Start IT eller tilsvarende kompetanse
- Muligheter etter fullføring: Søke jobb som frontend-utvikler eller fortsette til GET Prepared

## Verktøy og oppsett

For å komme i gang trenger du følgende:

- [Visual Studio Code](https://code.visualstudio.com/) - Anbefalt kodeeditor
- [Prettier](https://prettier.io/) - Code formatter for automatisk kodestil
- En moderne nettleser (Chrome, Firefox, Safari eller Edge)

Vi anbefaler å installere Prettier som en VS Code-extension for best mulig opplevelse under kursarbeidet.

## Navigering av repositoriet

Repositoriet er strukturert som følger:

```bash
Frontend2026.3/
├── README.md                              # Denne filen
├── Velkommen.md                           # Velkommen og introduksjonsside
├── .gitignore                             # Git ignore fil
├── 1. Responsivt design med HTML og CSS/  # Emne 1
│   ├── README.md                          # Emne 1 oversikt
│   ├── Uke 1/                             # Uke 1 materiale
│   │   ├── README.md                      # Uke 1 innhold
│   │   ├── [Leksjonsfiler].md             # Individuelle leksjoner
│   │   └── Eksempler/                     # HTML/CSS eksempler
│   ├── Uke 2/                             # Uke 2 materiale
│   │   ├── README.md                      # Uke 2 innhold
│   │   ├── [Leksjonsfiler].md             # Individuelle leksjoner
│   │   ├── Eksempler/                     # HTML/CSS eksempler
│   │   └── img/                           # Bilder for emne 1
│   └── Uke 3/                             # Uke 3 materiale
│       └── README.md                      # Uke 3 innhold
└── 2. Frameworkless Frontend Development, TypeScript og Funksjonell Programmering/  # Emne 2+3, del 1
    ├── README.md                          # Modul 2 oversikt
    ├── Uke 1/                             # Uke 1 materiale
    │   ├── README.md                      # Uke 1 innhold
    │   ├── [Leksjonsfiler].md             # Individuelle leksjoner
    │   ├── Eksempler/                     # Kodeeksempler
    │   └── Ekstra materiale/              # Valgfri fordypning
    └── Uke 2–9/                          # Videre modulmateriale
```

**Hvordan navigere:**

- Start med [Velkommen.md](../Velkommen.md) for generell introduksjon
- Gå til [1. Responsivt design med HTML og CSS](../1.%20Responsivt%20design%20med%20HTML%20og%20CSS/README.md) for første emne
- Gå til [2. Frameworkless Frontend Development, TypeScript og Funksjonell Programmering](../2.%20Frameworkless%20Frontend%20Development%2C%20TypeScript%20og%20Funksjonell%20Programmering/README.md) for andre emne
- Hver uke har sin egen README med leksjonsoversikt og lenker til materiale
- Eksempler er organisert i `Eksempler/` mapper under hver uke
- Arbeidsoppgaver og øvelser finnes i hver uke sin README

## Kurslitteratur

> Kurset er bygget på bøkene nedenfor – undervisningsmaterialet i mappene følger deres innhold, og bøkene er den naturlige litteraturen for hvert emne. Du kan lese dem parallelt med undervisningen eller bruke dem til fordypning. Nye utgaver av bøkene kan komme ut etter at kurset er publisert – undervisningsmaterialet i mappene gjelder da som den gjeldende versjonen.

### Emne 1 Responsive nettsider med HTML og CSS

- [Responsive Web Design with HTML5 and CSS - Fourth Edition, Ben Frain](https://www.adlibris.com/no/bok/responsive-web-design-with-html5-and-css-9781803242712)
  - [GitHub – benfrain/rwd4 (kode)](https://github.com/benfrain/rwd4)

### Emne 2 Objektorientert programmering i TypeScript og funksjonell programmering i JavaScript

- [Mastering TypeScript, Nathan Rozentals](https://www.adlibris.com/nb/bok/mastering-typescript-9781800564732)
  - [GitHub (kode) – 4th Edition](https://github.com/PacktPublishing/Mastering-TypeScript-Fourth-Edition)
- [Professor Frisby’s Mostly Adequate Guide To Functional Programming, Brian Lonsdorf](https://mostly-adequate.gitbook.io/mostly-adequate-guide/)
  - [GitHub (kode)](https://github.com/MostlyAdequate/mostly-adequate-guide)

### Emne 3 Single page applications

- [Frameworkless Front-End Development, Francesco Strazzullo](https://www.adlibris.com/nb/bok/frameworkless-front-end-development-9781484293508)
  - [GitHub (kode)](https://github.com/PacktPublishing/Frameworkless-Front-End-Development)
- [Vue.js 3 Design Patterns and Best Practices, Pablo David Garaguso, Olaf Zander](https://www.adlibris.com/nb/bok/vuejs-3-design-patterns-and-best-practices-9781803238074)
  - [GitHub (kode)](https://github.com/PacktPublishing/Vue.js-3-Design-Patterns-and-Best-Practices)

## Kursoversikt

Dette kurset er delt inn i tre hovedemner:

1. Responsive nettsider med HTML og CSS (5 SP)
2. Objektorientert programmering i TypeScript og funksjonell programmering i JavaScript (10 SP)
3. Single page applications med Vue.js og TypeScript (15 SP)

Det er mye overlapp mellom emnene, og de bygger på hverandre for å gi en helhetlig forståelse av frontend-utvikling.
Så vi har delt opp kurset i tre moduler for å strukturere læringen bedre.

Vi deler dette opp litt annerledes: 
1. 3 uker Emne 1 - Geir J
1. 17 uker Emne 2 og 3 
    1. 9 uker Del 1 - Frameworkless, TypeScript, Testing, Funksjonell programmering - Terje
    2. 8 uker Del 2 - Rammerverkene: Mest Vue, men også Nuxt, React, Angular, Svelte - Geir J

## Fremdriftsplan

Se [Ukeplan](../Ukeplan.md) for en detaljert oversikt over kursets fremdrift og hvordan en uke typisk ser ut.

### [1. HTML og CSS](../1.%20Responsivt%20design%20med%20HTML%20og%20CSS/README.md)

Dekker emne 1: Responsive nettsider med HTML og CSS

- [Uke 1](../1.%20Responsivt%20design%20med%20HTML%20og%20CSS/Uke%201/README.md)
- [Uke 2](../1.%20Responsivt%20design%20med%20HTML%20og%20CSS/Uke%202/README.md)
- [Uke 3](../1.%20Responsivt%20design%20med%20HTML%20og%20CSS/Uke%203/README.md)

### [2. Frameworkless, TypeScript og funksjonell programmering](../2.%20Frameworkless%20Frontend%20Development%2C%20TypeScript%20og%20Funksjonell%20Programmering/README.md)

Dekker emne 2 + 3, del 1: Frameworkless Frontend Development, TypeScript og funksjonell programmering

- [Uke 1](../2.%20Frameworkless%20Frontend%20Development%2C%20TypeScript%20og%20Funksjonell%20Programmering/Uke%201/README.md)
- [Uke 2](../2.%20Frameworkless%20Frontend%20Development%2C%20TypeScript%20og%20Funksjonell%20Programmering/Uke%202/README.md)
- [Uke 3](../2.%20Frameworkless%20Frontend%20Development%2C%20TypeScript%20og%20Funksjonell%20Programmering/Uke%203/README.md)
- [Uke 4](../2.%20Frameworkless%20Frontend%20Development%2C%20TypeScript%20og%20Funksjonell%20Programmering/Uke%204/README.md)
- [Uke 5](../2.%20Frameworkless%20Frontend%20Development%2C%20TypeScript%20og%20Funksjonell%20Programmering/Uke%205/README.md)
- [Uke 6](../2.%20Frameworkless%20Frontend%20Development%2C%20TypeScript%20og%20Funksjonell%20Programmering/Uke%206/README.md)
- [Uke 7](../2.%20Frameworkless%20Frontend%20Development%2C%20TypeScript%20og%20Funksjonell%20Programmering/Uke%207/README.md)
- [Uke 8](../2.%20Frameworkless%20Frontend%20Development%2C%20TypeScript%20og%20Funksjonell%20Programmering/Uke%208/README.md)
- [Uke 9](../2.%20Frameworkless%20Frontend%20Development%2C%20TypeScript%20og%20Funksjonell%20Programmering/Uke%209/README.md)

*Modul 3 (Single Page Applications med Vue) legges til når innholdet er klart.*
