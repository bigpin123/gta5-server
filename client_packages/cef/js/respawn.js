function set(data) {
    data = JSON.parse(data);
    $('.respaw_nO_nExit').css('display', 'grid');

    if (data[1] === true) $('.respawn_Fraction').css('display', 'grid');
    else $('.respawn_Fraction_no').css('display', 'grid');

    if (data[2] === true) $('.spawn_home').css('display', 'flex');
    else $('.spawn_home_no').css('display', 'grid');
}

function spawn(id) {
    mp.trigger('spawn', id);
}