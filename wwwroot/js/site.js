$(document).ready(function() {
    window.setInterval(function () {
        $("obf").each(function() {
            const el = $(this);
            let len = el.text().length;
            let output = "";
            for (let i = 0; i < len; i++)
                output += String.fromCharCode(randInt(64, 95));
            el.find('a').each(function() {
                $(this).text(output);
            });
        });
    }, 50);
});
