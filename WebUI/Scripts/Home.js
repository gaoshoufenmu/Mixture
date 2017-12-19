$(function () {
    $("#crude_pinyin").keyup(function (event) {
        if (event.keyCode === 13) {
            if ($("#crude_pinyin").val().trim() === "") {
                $("#crude_pinyin").focus()
            } else {
                $.get("api/pyutil?pinyin=" + encodeURI($("#crude_pinyin").val().trim()),
                    //$.get("api/values/5",
                    function (data) {

                        $("#pinyin_res").html(data);
                    });
            }
            return
        }
    })
    //$("#com_name").keyup(function (event) {
    //    if (event.keyCode === 13) {
    //        if ($("#com_name").val().trim() === "") {
    //            $("#com_name").focus()
    //        } else {
    //            $.get("api/segcom?comname=" + encodeURI($("#com_name").val().trim()),
    //                //$.get("api/values/5",
    //                function (data) {

    //                    $("#com_res").html(data);
    //                });
    //        }
    //        return
    //    }
    //})
    //$("#input_str_a").keyup(function (event) {
    //    if (event.keyCode === 13) {
    //        if ($("#input_str_a").val().trim() === "") {
    //            $("#input_str_a").focus()
    //        } else {
    //            $.get("api/rec?input=" + encodeURI($("#input_str_a").val().trim()),
    //                //$.get("api/values/5",
    //                function (data) {

    //                    $("#a_res").html(data);
    //                });
    //        }
    //        return
    //    }
    //})
    //$("#com_addr").keyup(function (event) {
    //    if (event.keyCode === 13) {
    //        if ($("#com_addr").val().trim() === "") {
    //            $("#com_addr").focus()
    //        } else {
    //            $.get("/api/segcom/segaddr?addr=" + encodeURI($("#com_addr").val().trim()),
    //                //$.get("api/values/5",
    //                function (data) {

    //                    $("#addr_res").html(data);
    //                });
    //        }
    //        return
    //    }
    //})
})

//主页模版
function homeLayout() {
    'use strict';
    init();
    function init() {

        $("#login").attr("href", "/usercenter/login?ReturnUrl=" + encodeURIComponent(location.href));
        $(document).click(function (o) {
            if (o.toElement !== null) {
                if (o.toElement.className !== "sec-txt") {
                    $(".sec-tips").removeClass("open");
                }
                if (o.toElement.className !== "city-cur" && o.toElement.className !== "cur" && o.toElement.className !== "icon-adr1") {
                    $(".sec-fm").removeClass("open-sec-adr");
                }
                if (o.toElement === $(".bg-fixed")) {
                    PhoneBr(function () {

                    }, function () {
                        $(".bg-fixed").hide();
                    })
                }

                $(".app").click(function (event) {
                    event.stopPropagation();
                });
                $(".qr-wrap").click(function (event) {
                    event.stopPropagation();
                });
                $(".toolbar").removeClass("open-qr");
                $(".toolbar").removeClass("open-app");
            }
        });

        //查询
        $("#pinyin_button").click(function () {
            if ($("#crude_pinyin").val().trim() === "") {
                $("#crude_pinyin").focus()
            } else {
                $.get("api/pyutil?pinyin=" + encodeURI($("#crude_pinyin").val().trim()),
                    //$.get("api/values/5",
                    function (data) {

                        $("#pinyin_res").html(data);
                    });
            }
        });

        //$("#seg_button").click(function () {
        //    if ($("#com_name").val().trim() === "") {
        //        $("#com_name").focus()
        //    } else {
        //        $.get("api/segcom?comname=" + encodeURI($("#com_name").val().trim()),
        //            //$.get("api/values/5",
        //            function (data) {

        //                $("#com_res").html(data);
        //            });
        //    }
        //});

        //$("#seg_addr_btn").click(function () {
        //    if ($("#com_addr").val().trim() === "") {
        //        $("#com_addr").focus()
        //    } else {
        //        $.get("/api/segcom/segaddr?addr=" + encodeURI($("#com_addr").val().trim()),
        //            //$.get("api/values/5",
        //            function (data) {

        //                $("#addr_res").html(data);
        //            });
        //    }
        //});


        //$("#recog_button_a").click(function () {
        //    if ($("#input_str_a").val().trim() === "") {
        //        $("#input_str_a").focus()
        //    } else {
        //        $.get("api/rec?input=" + encodeURI($("#input_str_a").val().trim()),
        //            //$.get("api/values/5",
        //            function (data) {

        //                $("#a_res").html(data);
        //            });
        //    }
        //});
    }
}