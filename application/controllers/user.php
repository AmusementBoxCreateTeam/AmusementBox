<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class User extends CI_Controller {

    public function __construct() {
        parent::__construct();
        $this->output->enable_profiler(TRUE);
        $this->load->model(array('users'));
    }

    public function index() {
        $data['list'] = $this->users->get_list();
        //表示用リストデータ
        foreach($data['list'] as $val){
            $val->age = (int)((date('Ymd') - date('Ymd',strtotime($val->birthday))) / 10000);
            if($val->gender == 0){
                $val->gender = '女';
            }else if($val->gender == 1){
                $val->gender = '男';
            }
        }
        $this->load->view('user/index.php', $data);
    }

}
