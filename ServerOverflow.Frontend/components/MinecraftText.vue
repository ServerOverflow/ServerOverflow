<template>
  <span class="minecraft-text" v-html="renderedContent"></span>
</template>

<script setup>
/*
  Note: this was written by DeepSeek V3 based on
  the original TextComponent implementation in C#
*/

const props = defineProps({
  translate: false,
  payload: ''
})

const language = props.translate ? await $fetch('/meta/language.json') : null;

const translate = (key, placeholders = []) => {
  if (language === null) return '[TRANSLATED]';
  let translation = language[key] || key
  placeholders.forEach((ph, i) => {
    translation = translation.replace(`%${i+1}%`, ph)
  })
  return translation
}

const COLOR_VALUES = {
  '0': '#000000', '1': '#0000AA', '2': '#00AA00', '3': '#00AAAA',
  '4': '#AA0000', '5': '#AA00AA', '6': '#FFAA00', '7': '#AAAAAA',
  '8': '#555555', '9': '#5555FF', 'a': '#55FF55', 'b': '#55FFFF',
  'c': '#FF5555', 'd': '#FF55FF', 'e': '#FFFF55', 'f': '#FFFFFF',
  'r': '#FFFFFF'
}

const renderedContent = computed(() => {
  const input = props.payload
  if (!input) return ''
  const parsed = JSON.parse(input)
  if (typeof parsed === 'string')
    return renderLegacyText(parsed)
  return renderComponent(parsed)
})

function renderComponent(component) {
  if (typeof component === 'string') {
    return renderLegacyText(component)
  }

  if (Array.isArray(component)) {
    return component.map(renderComponent).join('')
  }

  let html = ''

  if (component.translate) {
    const placeholders = component.with?.map(w => renderComponent(w)) || []
    return renderText(translate(component.translate, placeholders), {
      color: component.color,
      bold: component.bold,
      italic: component.italic,
      underlined: component.underlined,
      strikethrough: component.strikethrough,
      obfuscated: component.obfuscated
    })
  }

  if (component.text) {
    html += renderText(component.text, {
      color: component.color,
      bold: component.bold,
      italic: component.italic,
      underlined: component.underlined,
      strikethrough: component.strikethrough,
      obfuscated: component.obfuscated
    })
  }

  if (component.extra) {
    html += component.extra.map(renderComponent).join('')
  }

  return html
}

function renderLegacyText(text) {
  return renderText(text, {})
}

function renderText(text, baseStyles) {
  let html = ''
  let currentStyles = { ...baseStyles }
  let buffer = ''

  for (let i = 0; i < text.length; i++) {
    const char = text[i]

    if ((char === '§' || char === '&') && i + 1 < text.length) {
      if (buffer) {
        html += wrapText(buffer, currentStyles)
        buffer = ''
      }

      const code = text[++i].toLowerCase()

      if (COLOR_VALUES[code]) {
        currentStyles = {
          ...baseStyles,
          color: COLOR_VALUES[code]
        }
      }
      else if (code === 'l') currentStyles.bold = true
      else if (code === 'm') currentStyles.strikethrough = true
      else if (code === 'n') currentStyles.underlined = true
      else if (code === 'o') currentStyles.italic = true
      else if (code === 'k') currentStyles.obfuscated = true
      else if (code === 'r') {
        currentStyles = { ...baseStyles }
      }
    } else if (char === '\n') {
      if (buffer) {
        html += wrapText(buffer, currentStyles)
        buffer = ''
      }
      html += '<br>'
    } else {
      buffer += char
    }
  }

  if (buffer) {
    html += wrapText(buffer, currentStyles)
  }

  return html
}

function wrapText(text, styles) {
  if (!text) return ''

  const tag = styles.obfuscated ? "obf" : "span";
  const style = [
    styles.color ? `color:${styles.color}` : '',
    styles.bold ? 'font-weight:bold' : '',
    styles.italic ? 'font-style:italic' : '',
    styles.underlined ? 'text-decoration:underline' : '',
    styles.strikethrough ? 'text-decoration:line-through' : '',
    styles.obfuscated ? "obfuscated" : ''
  ].filter(Boolean).join(';')

  return style
      ? `<${tag} style="${style}">${escapeHtml(text)}</${tag}>`
      : escapeHtml(text)
}

function escapeHtml(text) {
  return text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
}
</script>

<style scoped>
.minecraft-text {
  font-family: "Minecraft", monospace !important;
}
</style>