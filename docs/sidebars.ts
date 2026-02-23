import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  tutorialSidebar: [
    'intro',
    {
      type: 'category',
      label: 'Getting Started',
      items: ['getting-started/installation', 'getting-started/quick-start', 'getting-started/your-first-result'],
    },
    {
      type: 'category',
      label: 'Core Concepts',
      items: ['core-concepts/result-type', 'core-concepts/error-type', 'core-concepts/Spur-oriented-programming'],
    },
    {
      type: 'category',
      label: 'Pipeline Operators',
      items: [
        'pipeline/then',
        'pipeline/map',
        'pipeline/validate',
        'pipeline/tap',
        'pipeline/recover',
        'pipeline/match',
      ],
    },
    {
      type: 'category',
      label: 'Integrations',
      items: [
        'integrations/aspnetcore',
        'integrations/entity-framework',
        'integrations/fluentvalidation',
        'integrations/mediatr',
      ],
    },
    {
      type: 'category',
      label: 'API Reference',
      items: ['api/error', 'api/result', 'api/unit'],
    },
  ],
};

export default sidebars;
