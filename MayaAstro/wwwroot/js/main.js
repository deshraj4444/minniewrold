$(document).ready(function(){
    // banner slider
    $('.as_banner_slider').slick({
        dots: true,
        speed:800,
        slidesToShow: 1,
        slidesToScroll: 1,
        prevArrow:'<button type="button" class="slick-prev as_btn"><span><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" preserveAspectRatio="xMidYMid" width="12" height="16" viewBox="0 0 12 16"> <path d="M12.003,15.996 L-0.000,7.997 L12.002,-0.001 L10.062,7.997 L12.003,15.996 ZM10.102,2.762 L2.246,7.997 L10.102,13.233 L8.832,7.997 L10.102,2.762 ZM3.824,7.997 L8.256,5.043 L7.539,7.997 L8.256,10.951 L3.824,7.997 Z" class="cls-1"/> </svg></span> Prev</button>',
        nextArrow:'<button type="button" class="slick-next as_btn">next <span><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" preserveAspectRatio="xMidYMid" width="8" height="12" viewBox="0 0 8 12"><path d="M8.000,5.998 L-0.002,11.997 L1.292,5.998 L-0.002,-0.001 L8.000,5.998 ZM1.265,9.924 L6.502,5.998 L1.265,2.071 L2.112,5.998 L1.265,9.924 ZM5.451,5.998 L2.496,8.213 L2.974,5.998 L2.496,3.783 L5.451,5.998 Z" class="cls-1"/> </svg></span></button>'
    });

  
$(window).scroll(function(){
            if($(this).scrollTop() > 100){
                $('.navbar').addClass('sticky')
            } else{
                $('.navbar').removeClass('sticky')
            }
        });
  
  
})

      