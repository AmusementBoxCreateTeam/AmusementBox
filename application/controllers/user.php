<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class User extends CI_Controller {

    public function __construct() {
        parent::__construct();
        $this->output->enable_profiler(TRUE);
        $this->load->model(array('users'));
        $this->load->library(array('pagination'));
    }

    public function index() {
        if (empty($_GET)) {
            //GETがない
            $search = '';
            $per_page = '';
        } else {
            $search = $this->input->get();
        }
        //valdiation
        $this->set_validation_rules();
        if ($this->form_validation->run() === FALSE) {
            $path = 'user/index';
            goto end;
        }
        $data['list'] = $this->users->get_list($search, $per_page);
        //表示用リストデータ
        foreach ($data['list'] as $val) {
            //年齢
            $val->age = (int) ((date('Ymd') - date('Ymd', strtotime($val->birthday))) / 10000);
            //性別の変換
            if ($val->gender == 0) {
                $val->gender = '女';
            } else if ($val->gender == 1) {
                $val->gender = '男';
            }
            /* 利用回数
              $val->use_count = $this->users->get_use_count($val->id);
             */
        }
        //ページング
        //全件数を取得
        $count_list = $this->users->get_count_list();
        $url = base_url() . 'index.php/user';
        $data['paging'] = $this->pagination($url, $count_list);
        $this->load->view('user/index.php', $data);

        //viewのロード
        end:
        $this->load->view($path);
    }

    public function detail($user_id = '') {

        if (empty($user_id)) {
            show_404();
            exit();
        }
    }

    /**
     * ページング設定
     * @param type $url
     * @param type $total_rows
     */
    public function pagination($url = '', $total_rows = '') {

        $config['first_link'] = '最初';
        $config['last_link'] = '最後';
        $config['page_query_string'] = TRUE;
        $config['base_url'] = $url;
        $config['total_rows'] = $total_rows;
        $config['per_page'] = 100;

        $this->pagination->initialize($config);

        echo $this->pagination->create_links();
    }

    /**
     * ヴァリデーションの設定
     */
    public function set_validation_rules() {

        $config = array();
        $config = array(
            array(
                'field' => 'gender',
                'label' => '性別',
                'rules' => 'trim'
            ),
            array(
                'field' => 'age_over',
                'label' => '年齢（以上）',
                'rules' => 'trim|required'
            ),
            array(
                'field' => 'age_under',
                'label' => '年齢（以下）',
                'rules' => 'trim'
            ),
            array(
                'field' => 'entry_date_over',
                'label' => '登録日（以上）',
                'rules' => 'trim'
            ),
            array(
                'field' => 'entry_date_under',
                'label' => '登録日（以下）',
                'rules' => 'trim'
            )
        );

        $this->form_validation->set_rules($config);
    }

}
