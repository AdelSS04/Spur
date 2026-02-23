# Spur Website & Documentation

This repository contains two websites for Spur:

1. **Landing Page** (`/website`) - Next.js static site with modern design
2. **Documentation** (`/docs`) - Docusaurus documentation site

## 🚀 Quick Start

### Prerequisites

- Node.js 20.x or later
- npm or yarn

### Landing Page (Next.js)

```bash
cd website
npm install
npm run dev
```

Visit: http://localhost:3000

### Documentation (Docusaurus)

```bash
cd docs
npm install
npm run start
```

Visit: http://localhost:3000

## 📦 Building for Production

### Landing Page

```bash
cd website
npm install
npm run build
```

Output: `website/out/`

### Documentation

```bash
cd docs
npm install
npm run build
```

Output: `docs/build/`

## 🌐 Deployment to GitHub Pages

### Automatic Deployment

The website deploys automatically when you push to `main` or `master` branch.

**GitHub Actions workflow** (`.github/workflows/deploy-website.yml`):
1. Builds the landing page (Next.js)
2. Builds the documentation (Docusaurus)
3. Combines both into one deployment
4. Deploys to GitHub Pages

### Setup GitHub Pages

1. Go to your repository settings
2. Navigate to **Pages** section
3. Under **Source**, select:
   - Source: **GitHub Actions**
4. Save settings

The website will be available at: `https://adelss04.github.io/Spur/`

### Manual Deployment

If you prefer manual deployment:

```bash
# Build both sites
cd website && npm run build && cd ..
cd docs && npm run build && cd ..

# Combine outputs
mkdir -p deploy
cp -r website/out/* deploy/
cp -r docs/build deploy/docs

# Deploy to GitHub Pages (using gh-pages package)
npx gh-pages -d deploy
```

## 📁 Project Structure

```
/website                 # Next.js Landing Page
├── app/                # Next.js 15 app directory
│   ├── page.tsx        # Main landing page
│   ├── layout.tsx      # Root layout
│   └── globals.css     # Global styles
├── components/         # React components
├── public/             # Static assets
├── next.config.ts      # Next.js configuration
├── tailwind.config.ts  # Tailwind CSS config
└── package.json

/docs                    # Docusaurus Documentation
├── docs/               # Markdown documentation
│   ├── intro.md        # Introduction
│   ├── getting-started/
│   ├── core-concepts/
│   ├── pipeline/
│   ├── integrations/
│   └── api/
├── blog/               # Blog posts
├── src/                # Custom React components
├── static/             # Static assets
├── docusaurus.config.ts
├── sidebars.ts         # Documentation sidebar
└── package.json
```

## 🎨 Customization

### Landing Page

**Colors**: Edit `website/tailwind.config.ts`
**Content**: Edit `website/app/page.tsx`
**Logo**: Replace `website/public/logo.svg`

### Documentation

**Theme**: Edit `docs/docusaurus.config.ts`
**Content**: Edit files in `docs/docs/`
**Sidebar**: Edit `docs/sidebars.ts`

## 📝 Creating Documentation

### Add New Page

1. Create a new `.md` or `.mdx` file in `docs/docs/`
2. Add frontmatter:

```md
---
sidebar_position: 1
title: Page Title
---

# Your Content Here
```

3. Update `docs/sidebars.ts` to include the new page

### Example Documentation Page

```md
---
sidebar_position: 1
---

# Quick Start

## Installation

\`\`\`bash
dotnet add package Spur
\`\`\`

## Your First Result

\`\`\`csharp
using Spur;

public Result<int> Divide(int a, int b)
{
    if (b == 0)
        return Error.Validation("Cannot divide by zero");

    return Result.Success(a / b);
}
\`\`\`
```

## 🔧 Troubleshooting

### Build Errors

**Issue**: `npm install` fails
- **Solution**: Delete `node_modules` and `package-lock.json`, then run `npm install` again

**Issue**: Next.js build fails with "Image Optimization"
- **Solution**: Already configured with `images: { unoptimized: true }` in `next.config.ts`

**Issue**: Docusaurus broken links
- **Solution**: Check all internal links and update `baseUrl` in `docusaurus.config.ts`

### Deployment Issues

**Issue**: GitHub Pages shows 404
- **Solution**:
  1. Check repository settings → Pages → Source is "GitHub Actions"
  2. Verify workflow completed successfully
  3. Wait 2-3 minutes for deployment propagation

**Issue**: CSS not loading on GitHub Pages
- **Solution**: Verify `baseUrl` in both `next.config.ts` and `docusaurus.config.ts` matches your repository name

## 🚢 Production Checklist

Before deploying to production:

- [ ] Update all URLs from `adelss04.github.io` to your domain
- [ ] Replace placeholder logo with actual Spur logo
- [ ] Test all navigation links
- [ ] Test responsive design (mobile, tablet, desktop)
- [ ] Run `npm run build` locally to check for errors
- [ ] Update meta tags and Open Graph images
- [ ] Enable GitHub Pages in repository settings
- [ ] Push to main/master branch to trigger deployment
- [ ] Verify deployment at `https://adelss04.github.io/Spur/`

## 📚 Additional Resources

- [Next.js Documentation](https://nextjs.org/docs)
- [Docusaurus Documentation](https://docusaurus.io/)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [GitHub Pages Documentation](https://docs.github.com/en/pages)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes in `/website` or `/docs`
4. Test locally
5. Submit a pull request

## 📄 License

MIT License - Same as Spur library

---

**Website URL**: https://adelss04.github.io/Spur/
**Repository**: https://github.com/AdelSS04/Spur

Built with ❤️ by [AdelSS04](https://github.com/AdelSS04)
