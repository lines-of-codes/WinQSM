import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";

// https://astro.build/config
export default defineConfig({
    site: "https://lines-of-codes.github.io",
    base: "QSMSharp",
    integrations: [
        starlight({
            title: "QSM",
            logo: {
                light: "./src/assets/QSMSharp_medium_wordmark.svg",
                dark: "./src/assets/QSMSharp_medium_light_wordmark.svg",
                replacesTitle: true,
            },
            customCss: ["./src/styles/custom.css"],
            social: [
                {
                    icon: "github",
                    label: "GitHub",
                    href: "https://github.com/lines-of-codes/QSMSharp",
                },
            ],
            defaultLocale: "root",
            locales: {
                root: {
                    label: "English",
                    lang: "en",
                },
                th: {
                    label: "ไทย",
                },
                lc: {
                    label: "lolcat",
                },
            },
            sidebar: [
                {
                    label: "QSMSharp",
                    items: [
                        {
                            slug: "common/guides/introduction",
                        },
                        {
                            slug: "common/guides/choosing-a-software",
                        },
                        {
                            slug: "common/guides/logo",
                        },
                    ],
                },
                {
                    label: "QSM.Windows Guides",
                    translations: {
                        th: "ไกด์ QSM.Windows",
                    },
                    items: [
                        {
                            slug: "qsmwin/guides/introduction",
                        },
                        {
                            slug: "qsmwin/guides/create-new-server",
                        },
                    ],
                },
                {
                    label: "QSM.Windows Reference",
                    translations: {
                        th: "เอกสารอ้างอิง QSM.Windows",
                    },
                    collapsed: true,
                    items: [
                        { autogenerate: { directory: "QSMWin/reference" } },
                    ],
                },
                {
                    label: "QSM.Web Guides",
                    translations: {
                        th: "ไกด์ QSM.Web",
                    },
                    items: [
                        {
                            slug: "qsmweb/guides/introduction",
                        },
                        { slug: "qsmweb/guides/security" },
                        {
                            label: "Installation",
                            items: [
                                { slug: "qsmweb/guides/linux-build" },
                                { slug: "qsmweb/guides/podman" },
                            ],
                        },
                    ],
                },
                {
                    label: "QSM.Web Reference",
                    translations: {
                        th: "เอกสารอ้างอิง QSM.Web",
                    },
                    collapsed: true,
                    items: [
                        { autogenerate: { directory: "QSMWeb/reference" } },
                    ],
                },
            ],
        }),
    ],
});
