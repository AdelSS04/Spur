import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const config: Config = {
  title: 'Spur',
  tagline: 'Spur-Oriented Programming for .NET',
  favicon: 'img/favicon.ico',

  url: 'https://adelss04.github.io',
  baseUrl: '/Spur/',

  organizationName: 'AdelSS04',
  projectName: 'Spur',

  onBrokenLinks: 'throw',

  markdown: {
    hooks: {
      onBrokenMarkdownLinks: 'warn',
    },
  },

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          routeBasePath: '/',
          editUrl: 'https://github.com/AdelSS04/Spur/tree/main/docs/',
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    image: 'img/social-card.jpg',
    navbar: {
      title: 'Spur',
      logo: {
        alt: 'Spur Logo',
        src: 'img/logo.svg',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'tutorialSidebar',
          position: 'left',
          label: 'Documentation',
        },

        {
          href: 'https://www.nuget.org/packages/Spur',
          label: 'NuGet',
          position: 'right',
        },
        {
          href: 'https://github.com/AdelSS04/Spur',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {
              label: 'Getting Started',
              to: '/',
            },
            {
              label: 'API Reference',
              to: '/api/error',
            },
          ],
        },
        {
          title: 'Community',
          items: [
            {
              label: 'GitHub Discussions',
              href: 'https://github.com/AdelSS04/Spur/discussions',
            },
            {
              label: 'Issues',
              href: 'https://github.com/AdelSS04/Spur/issues',
            },
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'NuGet',
              href: 'https://www.nuget.org/packages/Spur',
            },
            {
              label: 'GitHub',
              href: 'https://github.com/AdelSS04/Spur',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} Spur. Built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp', 'bash', 'json'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
