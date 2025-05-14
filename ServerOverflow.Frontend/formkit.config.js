import { defaultConfig } from '@formkit/vue'
import { createThemePlugin } from '@formkit/themes'
import { rootClasses } from './formkit.theme.ts'
import { genesisIcons } from '@formkit/icons'

export default defaultConfig({
    config: {
        rootClasses
    },
    plugins: [
        createThemePlugin()
    ],
    icons: {
        ...genesisIcons
    }
})