<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class User extends CI_Controller {

    public function __construct() {
        parent::__construct();
        #$this->output->enable_profiler(TRUE);
        $this->config->load('base');
        $this->load->model(array('users'));
        $this->load->library(array('pagination', 'conversion_age_birthday'));
        
        $this->logined->logincheck();
    }

    public function index() {

        $search = '';
        $data['per_page'] = '';
        if (!empty($_GET)) {
            $search = $this->input->get();
            $data['search'] = $search;
            $data['per_page'] = !empty($search['per_page']) ? $search['per_page'] : '';
        }

        $data['list'] = $this->users->get_list($search, $data['per_page']);
        //表示用リストデータ
        foreach ($data['list'] as $val) {
            //年齢
            $val->age = $this->conversion_age_birthday->conversion_age($val->birthday);
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
        //件数を取得
        $data['count_row'] = $this->users->get_count_row($search);
        $index = 0;
        $query = '';
        if (!empty($search)) {
            foreach ($search as $k => $v) {
                if (is_array($v)) {
                    foreach ($v as $v2) {
                        $query .= '&' . ($k . '[]') . '=' . ($v2);
                    }
                } else {
                    $query .= '&' . ($k) . '=' . ($v);
                }
                $index++;
            }
            $query = substr_replace($query, '?', 0, 1);
        }
        $url = base_url() . 'index.php/user';
        if (!empty($query)) {
            $url .= $query;
        }
        $data['paging'] = '';
        $data['paging'] = $this->pagination($url, $data['count_row']);
        $data['start_row'] = ($data['count_row'] === 0) ? 0 : $data['per_page'] + 1;
        $data['end_row'] = ($data['count_row'] === 0) ? 0 : $data['per_page'] + count($data['list']);
        $this->load->view('user/index', $data);
    }

    public function detail($user_id = '') {

        if (empty($user_id)) {
            show_404();
            exit();
        }
        $data['user'] = $this->users->get_user_detail($user_id);
        //性別の変換
        if ($data['user'][0]->gender == 0) {
            $data['user'][0]->gender = '女';
        } else if ($data['user'][0]->gender == 1) {
            $data['user'][0]->gender = '男';
        }
        $data['history'] = $this->users->get_history_detail($user_id);
        $this->load->view('user/detail', $data);
    }

    /**
     * ページング設定
     * @param type $url
     * @param type $total_rows
     */
    private function pagination($url = '', $total_rows = '') {

        $config['first_link'] = '最初';
        $config['last_link'] = '最後';
        $config['page_query_string'] = TRUE;
        $config['base_url'] = $url;
        $config['total_rows'] = $total_rows;
        $config['per_page'] = $this->config->item('disp_num');

        $this->pagination->initialize($config);

        return $this->pagination->create_links();
    }

    /**
     * validationの設定

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
     * 
     *      
     */
}
