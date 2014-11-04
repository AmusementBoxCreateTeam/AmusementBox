<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

/**
 * Description of Conversion_age_birthday
 *
 * @author DAI
 */
class Conversion_age_birthday {

    public function __construct() {
        
    }

    function conversion_age($date) {
        $age = (int) ((date('Ymd') - date('Ymd', strtotime($date))) / 10000);
        return $age;
    }

    function conversion_date($age) {
        $birthday = date('Y-m-d', strtotime(date('Ymd') - ($age * 10000)));
        return $birthday;
    }

}
