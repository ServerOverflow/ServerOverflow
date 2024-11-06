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

    $(".navbar-burger").click(function() {
        $(".navbar-burger").toggleClass("is-active");
        $(".navbar-menu").toggleClass("is-active");
    });

    $(".tab").on('click', function() {
        let obj = $(this);
        $(".tab-content").hide();
        $(`#${obj.data("target")}`).show();
        $(".tab").removeClass("is-active");
        obj.addClass("is-active");
    });

    $(".delete, .close-button").click(function() {
        let obj = $(this);
        let target = obj.data("target");
        if (target) {
            $(target).fadeTo(200, 0, function() { $(this).hide() });
            return;
        }
        
        obj.parent().slideUp();
    });

    $(".open-modal").click(function() {
        let obj = $(this); let target = obj.data("target");
        $(target).show().fadeTo(200, 1);
    });
});
